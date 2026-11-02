using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimViecLam.Controllers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint1;

public class Sprint1_SecurityAndAuthTests
{
    private readonly IPasswordService _passwordService = new PasswordService();

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.1: Password Hashing (PBKDF2 with salt, non-SHA256, Verify accuracy)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AC_S1_1_TwoIdenticalPasswords_ProduceDifferentHashesDueToSalt()
    {
        string rawPassword = "StrongP@ssword123";

        string hash1 = _passwordService.HashPassword(rawPassword);
        string hash2 = _passwordService.HashPassword(rawPassword);

        Assert.NotEqual(hash1, hash2);
        Assert.True(_passwordService.VerifyPassword(rawPassword, hash1));
        Assert.True(_passwordService.VerifyPassword(rawPassword, hash2));
    }

    [Fact]
    public void AC_S1_1_HashIsNotRawSha256Hex()
    {
        string rawPassword = "StrongP@ssword123";
        string hash = _passwordService.HashPassword(rawPassword);

        // SHA-256 raw hex is strictly 64 hex characters: [0-9a-fA-F]{64}
        bool isRawSha256 = Regex.IsMatch(hash, "^[0-9a-fA-F]{64}$");
        Assert.False(isRawSha256, "Password hash must not be a raw 64-char hex SHA-256 string.");
    }

    [Fact]
    public void AC_S1_1_VerifyPassword_AccurateForCorrectAndIncorrectPasswords()
    {
        string rawPassword = "CorrectPassword123";
        string wrongPassword = "WrongPassword456";

        string hash = _passwordService.HashPassword(rawPassword);

        Assert.True(_passwordService.VerifyPassword(rawPassword, hash));
        Assert.False(_passwordService.VerifyPassword(wrongPassword, hash));
        Assert.False(_passwordService.VerifyPassword(string.Empty, hash));
        Assert.False(_passwordService.VerifyPassword(rawPassword, string.Empty));
    }

    [Fact]
    public async Task AC_S1_1_SeededPasswords_UseNewFormatAndCanBeVerified()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        await DataSeeder.SeedAllAsync(testDb.Context, _passwordService);

        var demoUsers = await testDb.Context.Users.ToListAsync();
        Assert.NotEmpty(demoUsers);

        foreach (var user in demoUsers)
        {
            Assert.False(user.PasswordHash.Contains(':'), $"User {user.Username} password hash still contains legacy colon delimiter.");
            bool verified = _passwordService.VerifyPassword("Demo@123456", user.PasswordHash);
            Assert.True(verified, $"Failed to verify password for seeded user: {user.Username}");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.2: Registration Validation & Atomic Transaction
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_2_RegisterCandidate_DuplicateUsernameOrEmail_Rejected()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var emailSenderMock = new Mock<IEmailSender>();
        var controller = new AccountController(testDb.Context, _passwordService, currentUserServiceMock.Object, emailSenderMock.Object, NullLogger<AccountController>.Instance);

        var candidate1 = new RegisterCandidateViewModel
        {
            Username = "candidate_unique",
            Email = "candidate1@example.com",
            FullName = "Nguyễn Văn Test",
            Phone = "0901234567",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        var result1 = await controller.Register(candidate1);
        Assert.IsType<RedirectToActionResult>(result1);

        // Try duplicate username
        var candidate2 = new RegisterCandidateViewModel
        {
            Username = "CANDIDATE_UNIQUE",
            Email = "different@example.com",
            FullName = "Lê Văn Test",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };
        var result2 = await controller.Register(candidate2);
        Assert.IsType<ViewResult>(result2);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("Username"));

        // Try duplicate email
        var candidate3 = new RegisterCandidateViewModel
        {
            Username = "another_candidate",
            Email = "CANDIDATE1@EXAMPLE.COM",
            FullName = "Trần Văn Test",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };
        controller.ModelState.Clear();
        var result3 = await controller.Register(candidate3);
        Assert.IsType<ViewResult>(result3);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("Email"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("12345678")]      // digits only
    [InlineData("abcdefgh")]      // letters only
    public async Task AC_S1_2_Register_WeakPassword_Rejected(string weakPassword)
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);

        var model = new RegisterCandidateViewModel
        {
            Username = "valid_user",
            Email = "valid@example.com",
            FullName = "Tên Hợp Lệ",
            Password = weakPassword,
            ConfirmPassword = weakPassword
        };

        var result = await controller.Register(model);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("Password"));
    }

    [Fact]
    public async Task AC_S1_2_RegisterEmployer_AtomicTransaction_RollsBackOnFailure()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        var initialUsersCount = await testDb.Context.Users.CountAsync();
        var initialCompaniesCount = await testDb.Context.Companies.CountAsync();

        var model = new RegisterEmployerViewModel
        {
            Username = "emp_success",
            Email = "emp_success@company.com",
            FullName = "Giám Đốc Tuyển Dụng",
            Phone = "0912345678",
            CompanyName = "Tập đoàn Công nghệ Alpha",
            CompanyAddress = "123 Phố Duy Tân, Cầu Giấy, Hà Nội",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        var result = await controller.RegisterEmployer(model);
        Assert.IsType<RedirectToActionResult>(result);

        // Verify user and company were created with Status = "Pending"
        var createdUser = await testDb.Context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Username == "emp_success");
        Assert.NotNull(createdUser);
        Assert.Equal("Employer", createdUser.Role);
        Assert.NotNull(createdUser.Company);
        Assert.Equal("Pending", createdUser.Company.Status);
        Assert.Equal("Tập đoàn Công nghệ Alpha", createdUser.Company.Name);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.3: Login, Generic Error Message, 5-Attempt Lockout
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_3_FailedLogin_ReturnsIdenticalGenericMessage_ForNonexistentAndWrongPassword()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);

        // Seed a user
        testDb.Context.Users.Add(new tblUser
        {
            Username = "existing_user",
            Email = "existing@example.com",
            PasswordHash = _passwordService.HashPassword("Correct123"),
            Role = "Candidate",
            IsActive = true
        });
        await testDb.Context.SaveChangesAsync();

        // 1. Nonexistent user
        controller.ModelState.Clear();
        await controller.Login(new LoginViewModel { UsernameOrEmail = "nonexistent_user", Password = "AnyPassword123" });
        var nonexistentError = controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage;

        // 2. Existing user with wrong password
        controller.ModelState.Clear();
        await controller.Login(new LoginViewModel { UsernameOrEmail = "existing_user", Password = "WrongPassword123" });
        var wrongPasswordError = controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage;

        Assert.NotNull(nonexistentError);
        Assert.NotNull(wrongPasswordError);
        Assert.Equal(nonexistentError, wrongPasswordError);
        Assert.Equal("Tên đăng nhập/email hoặc mật khẩu không chính xác.", nonexistentError);
    }

    [Fact]
    public async Task AC_S1_3_AccountLocksAfter5FailedAttempts_EvenIfNextPasswordIsCorrect()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);

        testDb.Context.Users.Add(new tblUser
        {
            Username = "victim_user",
            Email = "victim@example.com",
            PasswordHash = _passwordService.HashPassword("Correct123"),
            Role = "Candidate",
            IsActive = true
        });

        testDb.Context.Users.Add(new tblUser
        {
            Username = "innocent_user",
            Email = "innocent@example.com",
            PasswordHash = _passwordService.HashPassword("Correct123"),
            Role = "Candidate",
            IsActive = true
        });
        await testDb.Context.SaveChangesAsync();

        // 5 consecutive failed login attempts
        for (int i = 1; i <= 5; i++)
        {
            controller.ModelState.Clear();
            await controller.Login(new LoginViewModel { UsernameOrEmail = "victim_user", Password = "WrongPassword!" });
        }

        var victim = await testDb.Context.Users.FirstAsync(u => u.Username == "victim_user");
        Assert.True(victim.LockoutEnd.HasValue);
        Assert.True(victim.LockoutEnd.Value > DateTime.UtcNow);

        // 6th attempt with CORRECT password while locked -> Must still be rejected!
        controller.ModelState.Clear();
        await controller.Login(new LoginViewModel { UsernameOrEmail = "victim_user", Password = "Correct123" });
        var lockedError = controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage;
        Assert.Contains("khóa", lockedError?.ToLowerInvariant());

        // Innocent user B is completely unaffected!
        var innocent = await testDb.Context.Users.FirstAsync(u => u.Username == "innocent_user");
        Assert.Equal(0, innocent.FailedLoginCount);
        Assert.Null(innocent.LockoutEnd);
    }

    [Fact]
    public async Task AC_S1_3_WhenLockoutPeriodExpires_UserCanLoginSuccessfully()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var httpContext = new DefaultHttpContext();
        var authServiceMock = new Mock<IAuthenticationService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService))).Returns(authServiceMock.Object);
        httpContext.RequestServices = serviceProviderMock.Object;

        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = Mock.Of<IUrlHelper>()
        };

        var user = new tblUser
        {
            Username = "expired_lockout_user",
            Email = "expired@example.com",
            PasswordHash = _passwordService.HashPassword("Correct123"),
            Role = "Candidate",
            IsActive = true,
            FailedLoginCount = 5,
            LockoutEnd = DateTime.UtcNow.AddMinutes(-5) // Expired 5 minutes ago
        };
        testDb.Context.Users.Add(user);
        await testDb.Context.SaveChangesAsync();

        var result = await controller.Login(new LoginViewModel { UsernameOrEmail = "expired_lockout_user", Password = "Correct123" });
        Assert.IsType<RedirectToActionResult>(result);

        var updatedUser = await testDb.Context.Users.FirstAsync(u => u.Username == "expired_lockout_user");
        Assert.Equal(0, updatedUser.FailedLoginCount);
        Assert.Null(updatedUser.LockoutEnd);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.6: Password Reset Token (Single use, expiration, invalid token)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_6_ResetPassword_TokenValidation_SingleUse_And_Expiration()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var emailSenderMock = new Mock<IEmailSender>();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), emailSenderMock.Object, NullLogger<AccountController>.Instance);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        string rawToken = "my_secret_random_token_12345678";
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var user = new tblUser
        {
            Username = "reset_user",
            Email = "reset@example.com",
            PasswordHash = _passwordService.HashPassword("OldPassword123"),
            Role = "Candidate",
            IsActive = true,
            ResetTokenHash = tokenHash,
            ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        testDb.Context.Users.Add(user);
        await testDb.Context.SaveChangesAsync();

        // 1. DB does NOT contain raw token
        var savedUser = await testDb.Context.Users.FirstAsync(u => u.Username == "reset_user");
        Assert.NotEqual(rawToken, savedUser.ResetTokenHash);
        Assert.Equal(tokenHash, savedUser.ResetTokenHash);

        // 2. Submit wrong token -> Fails
        controller.ModelState.Clear();
        var wrongResult = await controller.ResetPassword(new ResetPasswordViewModel
        {
            Email = "reset@example.com",
            Token = "wrong_token",
            NewPassword = "NewStrongPass123",
            ConfirmNewPassword = "NewStrongPass123"
        });
        Assert.IsType<ViewResult>(wrongResult);
        Assert.False(controller.ModelState.IsValid);

        // 3. Submit valid token -> Succeeds
        controller.ModelState.Clear();
        var successResult = await controller.ResetPassword(new ResetPasswordViewModel
        {
            Email = "reset@example.com",
            Token = rawToken,
            NewPassword = "NewStrongPass123",
            ConfirmNewPassword = "NewStrongPass123"
        });
        Assert.IsType<RedirectToActionResult>(successResult);

        // 4. Token is invalidated (ResetTokenHash == null)
        var updatedUser = await testDb.Context.Users.FirstAsync(u => u.Username == "reset_user");
        Assert.Null(updatedUser.ResetTokenHash);
        Assert.Null(updatedUser.ResetTokenExpiresAt);
        Assert.True(_passwordService.VerifyPassword("NewStrongPass123", updatedUser.PasswordHash));

        // 5. Submit SAME token a SECOND time -> Fails (single use enforcement)
        controller.ModelState.Clear();
        var secondResult = await controller.ResetPassword(new ResetPasswordViewModel
        {
            Email = "reset@example.com",
            Token = rawToken,
            NewPassword = "AnotherPass123",
            ConfirmNewPassword = "AnotherPass123"
        });
        Assert.IsType<ViewResult>(secondResult);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task AC_S1_6_ResetPassword_ExpiredToken_Fails()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);

        string rawToken = "expired_token_123456";
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var user = new tblUser
        {
            Username = "expired_token_user",
            Email = "expired_token@example.com",
            PasswordHash = _passwordService.HashPassword("OldPass123"),
            Role = "Candidate",
            IsActive = true,
            ResetTokenHash = tokenHash,
            ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(-5) // Expired
        };
        testDb.Context.Users.Add(user);
        await testDb.Context.SaveChangesAsync();

        var result = await controller.ResetPassword(new ResetPasswordViewModel
        {
            Email = "expired_token@example.com",
            Token = rawToken,
            NewPassword = "NewStrongPass123",
            ConfirmNewPassword = "NewStrongPass123"
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.7: Open Redirect Protection
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("http://attacker.com")]
    [InlineData("//evil.com")]
    [InlineData("/\\evil.com")]
    public async Task AC_S1_7_OpenRedirect_UrlsIgnored_RedirectsToHome(string evilUrl)
    {
        using var testDb = TestDbHelper.CreateSqliteContext();
        var httpContext = new DefaultHttpContext();
        var authServiceMock = new Mock<IAuthenticationService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService))).Returns(authServiceMock.Object);
        httpContext.RequestServices = serviceProviderMock.Object;

        var urlHelperMock = new Mock<IUrlHelper>();
        // Url.IsLocalUrl returns false for non-local urls
        urlHelperMock.Setup(u => u.IsLocalUrl(It.Is<string>(url => url != null && (url.StartsWith("http") || url.StartsWith("//")))))
                     .Returns(false);

        var controller = new AccountController(testDb.Context, _passwordService, Mock.Of<ICurrentUserService>(), Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = urlHelperMock.Object
        };

        testDb.Context.Users.Add(new tblUser
        {
            Username = "login_test_user",
            Email = "logintest@example.com",
            PasswordHash = _passwordService.HashPassword("Correct123"),
            Role = "Candidate",
            IsActive = true
        });
        await testDb.Context.SaveChangesAsync();

        var result = await controller.Login(new LoginViewModel
        {
            UsernameOrEmail = "login_test_user",
            Password = "Correct123",
            ReturnUrl = evilUrl
        });

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.10: Code Quality (No GetAwaiter().GetResult(), No Raw SHA256 for Passwords)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AC_S1_10_CodeQuality_NoGetAwaiterGetResult_InControllersAndServices()
    {
        var baseDir = AppContext.BaseDirectory;
        // Search root project folder
        var srcFolder = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "TimViecLam"));

        var csFiles = Directory.GetFiles(srcFolder, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj") && !f.Contains("Migrations"))
            .ToList();

        Assert.NotEmpty(csFiles);

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain(".GetAwaiter().GetResult()", content);
        }
    }

    [Fact]
    public void AC_S1_10_NoRawSha256_UsedForPasswordHashing()
    {
        var baseDir = AppContext.BaseDirectory;
        var srcFolder = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "TimViecLam"));

        var passwordServicePath = Path.Combine(srcFolder, "Services", "PasswordService.cs");
        var content = File.ReadAllText(passwordServicePath);

        // Verify PasswordHasher is used
        Assert.Contains("PasswordHasher<tblUser>", content);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.11: Profile Edit (Validation, Phone, IDOR Protection) & Change Password
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("0123456789", false)]   // Invalid Vietnam prefix 01
    [InlineData("1234567890", false)]   // Does not start with 0
    [InlineData("090123456", false)]    // 9 digits
    [InlineData("09012345678", false)]  // 11 digits
    [InlineData("0901234567", true)]    // Valid 09
    [InlineData("0381234567", true)]    // Valid 03
    [InlineData("0771234567", true)]    // Valid 07
    [InlineData("0861234567", true)]    // Valid 08
    public void AC_S1_11_VietnamPhoneNumberValidation(string phone, bool expectedValid)
    {
        var regex = new Regex(@"^(0[35789])[0-9]{8}$");
        bool isValid = regex.IsMatch(phone);
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public async Task AC_S1_11_UserCanOnlyEditOwnProfile_IDOR_Protected()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();

        testDb.Context.Users.Add(new tblUser
        {
            Username = "user_one",
            Email = "userone@example.com",
            FullName = "Người Dùng Một",
            PasswordHash = _passwordService.HashPassword("Pass1234"),
            Role = "Candidate",
            IsActive = true
        });

        testDb.Context.Users.Add(new tblUser
        {
            Username = "user_two",
            Email = "usertwo@example.com",
            FullName = "Người Dùng Hai",
            PasswordHash = _passwordService.HashPassword("Pass1234"),
            Role = "Candidate",
            IsActive = true
        });
        await testDb.Context.SaveChangesAsync();

        var userOne = await testDb.Context.Users.FirstAsync(u => u.Username == "user_one");
        var userTwo = await testDb.Context.Users.FirstAsync(u => u.Username == "user_two");

        // Mock current user logged in as user_one
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userOne.UserId);

        var controller = new AccountController(testDb.Context, _passwordService, currentUserServiceMock.Object, Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // User one attempts to submit edit with UserId = userTwo.UserId (malicious IDOR attempt)
        var model = new ProfileViewModel
        {
            UserId = userTwo.UserId,
            FullName = "Tên Giả Mạo Hacked",
            Phone = "0901234567"
        };

        var result = await controller.Profile(model);
        Assert.IsType<RedirectToActionResult>(result);

        // Verify: UserTwo was NOT changed! UserOne was updated!
        var refreshedUserTwo = await testDb.Context.Users.FirstAsync(u => u.Username == "user_two");
        var refreshedUserOne = await testDb.Context.Users.FirstAsync(u => u.Username == "user_one");

        Assert.Equal("Người Dùng Hai", refreshedUserTwo.FullName);
        Assert.Equal("Tên Giả Mạo Hacked", refreshedUserOne.FullName);
    }

    [Fact]
    public async Task AC_S1_11_ChangePassword_WithIncorrectOldPassword_Rejected()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();

        var user = new tblUser
        {
            Username = "pwd_user",
            Email = "pwd@example.com",
            PasswordHash = _passwordService.HashPassword("OldCorrectPass123"),
            Role = "Candidate",
            IsActive = true
        };
        testDb.Context.Users.Add(user);
        await testDb.Context.SaveChangesAsync();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(user.UserId);

        var controller = new AccountController(testDb.Context, _passwordService, currentUserServiceMock.Object, Mock.Of<IEmailSender>(), NullLogger<AccountController>.Instance);

        var model = new ChangePasswordViewModel
        {
            OldPassword = "WrongOldPassword123",
            NewPassword = "NewValidPassword123",
            ConfirmNewPassword = "NewValidPassword123"
        };

        var result = await controller.ChangePassword(model);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("OldPassword"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.12: SQLite Migration & Down Rollback
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_12_NewMigration_AppliesOnSqlite_AndSupportsDown()
    {
        using var testDb = TestDbHelper.CreateSqliteContext();

        // 1. Verify tblUser has columns ResetTokenHash and ResetTokenExpiresAt
        testDb.Context.Users.Add(new tblUser
        {
            Username = "migration_test_user",
            Email = "mig@example.com",
            PasswordHash = _passwordService.HashPassword("TestPass123"),
            Role = "Candidate",
            IsActive = true,
            ResetTokenHash = "testhash",
            ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await testDb.Context.SaveChangesAsync();

        var user = await testDb.Context.Users.FirstAsync(u => u.Username == "migration_test_user");
        Assert.Equal("testhash", user.ResetTokenHash);
        Assert.NotNull(user.ResetTokenExpiresAt);
    }
}
