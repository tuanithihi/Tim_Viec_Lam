using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Controllers;

/// <summary>
/// Controller xử lý đăng ký, đăng nhập, đăng xuất, hồ sơ và quản lý mật khẩu.
/// Route thân thiện được khai báo tập trung trong Program.cs.
/// </summary>
[EnableRateLimiting("auth-policy")]
public class AccountController : Controller
{
    private readonly DataContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        DataContext context,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        IEmailSender emailSender,
        ILogger<AccountController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐĂNG KÝ ỨNG VIÊN (Candidate)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterCandidateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_passwordService.ValidatePassword(model.Password, out var passErr))
        {
            ModelState.AddModelError(nameof(model.Password), passErr!);
            return View(model);
        }

        var normalizedUsername = model.Username.Trim().ToLower();
        var normalizedEmail = model.Email.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã được sử dụng. Vui lòng chọn tên khác.");
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "Địa chỉ email đã được đăng ký. Vui lòng sử dụng email khác.");
            return View(model);
        }

        var user = new tblUser
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            Phone = model.Phone?.Trim(),
            PasswordHash = _passwordService.HashPassword(model.Password),
            Role = "Candidate",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đăng ký tài khoản ứng viên thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐĂNG KÝ NHÀ TUYỂN DỤNG (Employer) ─ Transaction nguyên tử
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult RegisterEmployer()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterEmployer(RegisterEmployerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_passwordService.ValidatePassword(model.Password, out var passErr))
        {
            ModelState.AddModelError(nameof(model.Password), passErr!);
            return View(model);
        }

        var normalizedUsername = model.Username.Trim().ToLower();
        var normalizedEmail = model.Email.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã được sử dụng. Vui lòng chọn tên khác.");
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "Địa chỉ email đã được đăng ký. Vui lòng sử dụng email khác.");
            return View(model);
        }

        // Tạo User và tblCompany trong MỘT Transaction nguyên tử (AC-S1.2)
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new tblUser
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    FullName = model.FullName.Trim(),
                    Phone = model.Phone?.Trim(),
                    PasswordHash = _passwordService.HashPassword(model.Password),
                    Role = "Employer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Tạo Slug công ty duy nhất
                var baseSlug = SlugHelper.Slugify(model.CompanyName);
                if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "cong-ty";
                var slug = baseSlug;
                int suffix = 1;
                while (await _context.Companies.AnyAsync(c => c.Slug == slug))
                {
                    slug = $"{baseSlug}-{suffix++}";
                }

                var company = new tblCompany
                {
                    UserId = user.UserId,
                    Name = model.CompanyName.Trim(),
                    Slug = slug,
                    TaxCode = model.TaxCode?.Trim(),
                    Address = model.CompanyAddress.Trim(),
                    Phone = model.CompanyPhone?.Trim(),
                    Email = model.CompanyEmail?.Trim(),
                    CompanySize = model.CompanySize,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        TempData["Success"] = "Đăng ký tài khoản nhà tuyển dụng thành công! Hồ sơ doanh nghiệp đang chờ duyệt.";
        return RedirectToAction(nameof(Login));
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐĂNG NHẬP ─ Chống Timing Attack, Khóa tạm sau 5 lần sai
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var query = model.UsernameOrEmail.Trim().ToLower();

        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == query || u.Email.ToLower() == query);

        // 1. Chống Timing Attack: nếu không có tài khoản, băm giả lập thời gian phản hồi tương đương
        if (user == null)
        {
            _passwordService.VerifyPassword("DummyPassword123", "AQAAAAIAAYagAAAAEP0dummyHashSaltValueForTimingAttackDefense==");
            ModelState.AddModelError(string.Empty, "Tên đăng nhập/email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // 2. Kiểm tra khóa tài khoản tạm thời
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau.");
            return View(model);
        }

        // 3. Kiểm tra kích hoạt tài khoản
        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
            return View(model);
        }

        // 4. Kiểm tra mật khẩu
        bool isPasswordValid = _passwordService.VerifyPassword(model.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await _context.SaveChangesAsync();

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau.");
            }
            else
            {
                // Thông báo chung giống hệt cho "không tồn tại" và "sai mật khẩu" (AC-S1.3)
                ModelState.AddModelError(string.Empty, "Tên đăng nhập/email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // 5. Đăng nhập thành công -> Reset FailedLoginCount và LockoutEnd
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 6. Khởi tạo Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName ?? user.Username)
        };

        if (user.Role == "Employer" && user.Company != null)
        {
            claims.Add(new Claim("CompanyId", user.Company.CompanyId.ToString()));
            claims.Add(new Claim("CompanyStatus", user.Company.Status));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        // 7. Chống Open Redirect: chỉ nhận URL nội bộ, loại bỏ protocol-relative URL
        if (!string.IsNullOrEmpty(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl) &&
            !model.ReturnUrl.StartsWith("//") &&
            !model.ReturnUrl.StartsWith("/\\"))
        {
            return Redirect(model.ReturnUrl);
        }

        if (user.Role == "SuperAdmin")
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        return RedirectToAction("Index", "Home");
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐĂNG XUẤT ─ Chỉ chấp nhận POST (AC-S1.8)
    // ═══════════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [ActionName("Logout")]
    public IActionResult LogoutGet()
    {
        // GET đăng xuất không đăng xuất được (chuyển hướng an toàn về trang chủ)
        return RedirectToAction("Index", "Home");
    }

    // ═══════════════════════════════════════════════════════════════
    //  HỒ SƠ TÀI KHOẢN (Xem & Sửa cá nhân ─ Chống IDOR)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        int? currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Challenge();

        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId.Value);

        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FullName = user.FullName ?? string.Empty,
            Phone = user.Phone,
            Avatar = user.Avatar,
            Company = user.Company
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        int? currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Challenge();

        // Chống IDOR: Chỉ sửa hồ sơ của chính mình (AC-S1.11)
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId.Value);

        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Username = user.Username;
            model.Email = user.Email;
            model.Role = user.Role;
            model.Company = user.Company;
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Phone = model.Phone?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction(nameof(Profile));
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐỔI MẬT KHẨU
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        int? currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Challenge();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId.Value);
        if (user == null) return NotFound();

        // Kiểm tra mật khẩu cũ
        if (!_passwordService.VerifyPassword(model.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu hiện tại không chính xác.");
            return View(model);
        }

        // Kiểm tra chính sách mật khẩu mới
        if (!_passwordService.ValidatePassword(model.NewPassword, out var passErr))
        {
            ModelState.AddModelError(nameof(model.NewPassword), passErr!);
            return View(model);
        }

        user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Profile));
    }

    // ═══════════════════════════════════════════════════════════════
    //  QUÊN MẬT KHẨU (Gửi token ngẫu nhiên qua IEmailSender)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.IsActive);

        if (user != null)
        {
            // Sinh token ngẫu nhiên mạnh (32 bytes)
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var rawToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();

            // Băm token bằng SHA256 để lưu DB (DB không chứa token thô)
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

            user.ResetTokenHash = tokenHash;
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30); // Hết hạn 30 phút
            await _context.SaveChangesAsync();

            var resetUrl = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token = rawToken }, Request.Scheme)
                           ?? $"/dat-lai-mat-khau?email={user.Email}&token={rawToken}";

            await _emailSender.SendEmailAsync(user.Email, "Đặt lại mật khẩu - TimViecLam",
                $"Nhấn vào liên kết sau để đặt lại mật khẩu của bạn (có hiệu lực 30 phút): <a href='{resetUrl}'>{resetUrl}</a>");
        }

        // Thông báo chung chung không tiết lộ sự tồn tại của email
        ViewBag.Message = "Nếu địa chỉ email tồn tại trên hệ thống, hướng dẫn đặt lại mật khẩu đã được gửi đến bạn.";
        return View();
    }

    // ═══════════════════════════════════════════════════════════════
    //  ĐẶT LẠI MẬT KHẨU (Token dùng 1 lần, hết hạn 30 phút)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_passwordService.ValidatePassword(model.NewPassword, out var passErr))
        {
            ModelState.AddModelError(nameof(model.NewPassword), passErr!);
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        // Băm token đầu vào để đối soát với DB
        var submittedTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Token.Trim().ToLowerInvariant()))).ToLowerInvariant();

        if (user == null ||
            string.IsNullOrEmpty(user.ResetTokenHash) ||
            user.ResetTokenHash.ToLowerInvariant() != submittedTokenHash ||
            !user.ResetTokenExpiresAt.HasValue ||
            user.ResetTokenExpiresAt.Value < DateTime.UtcNow)
        {
            // Token sai, quá hạn, hoặc dùng lần 2 (ResetTokenHash đã bị null hóa) (AC-S1.6)
            ModelState.AddModelError(string.Empty, "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }

        // Đặt lại mật khẩu và hủy token (dùng 1 lần duy nhất)
        user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
        user.ResetTokenHash = null;
        user.ResetTokenExpiresAt = null;
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }
}
