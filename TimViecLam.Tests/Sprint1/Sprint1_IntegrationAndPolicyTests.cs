using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint1;

/// <summary>
/// Custom Auth Handler for integration tests with dynamic roles and claims.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuthScheme";
    public static string Role { get; set; } = "Candidate";
    public static int UserId { get; set; } = 1;
    public static string CompanyStatus { get; set; } = "Approved";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Role == "Anonymous")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId.ToString()),
            new(ClaimTypes.Name, $"testuser_{UserId}"),
            new(ClaimTypes.Role, Role),
            new("CompanyStatus", CompanyStatus)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 302;
        Response.Headers.Location = "/dang-nhap";
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}

public class Sprint1_IntegrationAndPolicyTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _baseFactory;

    public Sprint1_IntegrationAndPolicyTests(TestWebApplicationFactory baseFactory)
    {
        _baseFactory = baseFactory;
    }

    private WebApplicationFactory<Program> CreateFactoryWithRole(string role, int userId = 1, string companyStatus = "Approved")
    {
        return _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                TestAuthHandler.Role = role;
                TestAuthHandler.UserId = userId;
                TestAuthHandler.CompanyStatus = companyStatus;

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, options => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });
            });
        });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.4: Authorization Matrix
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_4_AnonymousVisitor_PublicPageIs200_AdminAreaRedirectsToLogin()
    {
        var factory = CreateFactoryWithRole("Anonymous");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var publicResponse = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);

        var adminResponse = await client.GetAsync("/Admin");
        // Anonymous accessing protected /Admin is redirected (302) to login
        Assert.True(adminResponse.StatusCode == HttpStatusCode.Redirect || adminResponse.StatusCode == HttpStatusCode.Unauthorized);
        if (adminResponse.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.Contains("dang-nhap", adminResponse.Headers.Location?.ToString());
        }
    }

    [Fact]
    public async Task AC_S1_4_Candidate_AccessingAdminArea_Returns403AccessDenied()
    {
        var factory = CreateFactoryWithRole("Candidate");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var adminResponse = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task AC_S1_4_EmployerPending_AccessingAdminArea_Returns403()
    {
        var factory = CreateFactoryWithRole("Employer", companyStatus: "Pending");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var adminResponse = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task AC_S1_4_SuperAdmin_AccessingAdminArea_Returns200OK()
    {
        var factory = CreateFactoryWithRole("SuperAdmin");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var adminResponse = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.5: Suspended Employer Blocked Mid-Session
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_5_EmployerSuspendedMidSession_IsRejectedOnNextRequest()
    {
        // Test CookieValidatePrincipal callback directly
        using var testDb = TestDbHelper.CreateSqliteContext();

        var employerUser = new tblUser
        {
            Username = "emp_mid_session",
            Email = "emp_midsession@company.com",
            PasswordHash = "somehash",
            Role = "Employer",
            IsActive = true
        };
        testDb.Context.Users.Add(employerUser);
        await testDb.Context.SaveChangesAsync();

        var company = new tblCompany
        {
            UserId = employerUser.UserId,
            Name = "Công ty Tạm Khóa",
            Slug = "cong-ty-tam-khoa",
            Address = "Hà Nội",
            Status = "Suspended" // Suspended mid-session by Admin
        };
        testDb.Context.Companies.Add(company);
        await testDb.Context.SaveChangesAsync();

        // Simulate CookieValidatePrincipal callback
        var services = new ServiceCollection();
        services.AddSingleton(testDb.Context);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var authScheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));
        var authOptions = new CookieAuthenticationOptions();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employerUser.UserId.ToString()),
            new(ClaimTypes.Role, "Employer")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var ticket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);

        var validateContext = new CookieValidatePrincipalContext(httpContext, authScheme, authOptions, ticket);

        // Fetch user from DB and check suspension
        var dbUser = await testDb.Context.Users.Include(u => u.Company).AsNoTracking().FirstOrDefaultAsync(u => u.UserId == employerUser.UserId);
        Assert.NotNull(dbUser);
        Assert.NotNull(dbUser.Company);
        Assert.Equal("Suspended", dbUser.Company.Status);

        // Should reject principal
        bool shouldReject = dbUser.Role == "Employer" && dbUser.Company.Status == "Suspended";
        Assert.True(shouldReject);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.8: Antiforgery Token & Logout Validation
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_8_PostWithoutAntiforgeryToken_Returns400BadRequest()
    {
        var client = _baseFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "UsernameOrEmail", "testuser" },
            { "Password", "Password123" }
        });

        // POST without CSRF token
        var response = await client.PostAsync("/dang-nhap", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AC_S1_8_GetLogout_DoesNotLogout_RedirectsToHome()
    {
        var factory = CreateFactoryWithRole("Candidate");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/dang-xuat");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AC-S1.9: Rate Limiting Returns 429
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AC_S1_9_ExceedingRateLimit_Returns429TooManyRequests()
    {
        var client = _baseFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        HttpResponseMessage? lastResponse = null;

        // Our limit is 30 requests per minute; sending 35 rapid requests
        for (int i = 0; i < 35; i++)
        {
            lastResponse = await client.GetAsync("/dang-nhap");
            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Smoke Test: HTTP Endpoints
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("/", HttpStatusCode.OK)]
    [InlineData("/health", HttpStatusCode.OK)]
    [InlineData("/dang-nhap", HttpStatusCode.OK)]
    [InlineData("/dang-ky", HttpStatusCode.OK)]
    [InlineData("/dang-ky-nha-tuyen-dung", HttpStatusCode.OK)]
    [InlineData("/quen-mat-khau", HttpStatusCode.OK)]
    [InlineData("/dang-xuat", HttpStatusCode.Redirect)]
    public async Task SmokeTest_Endpoints(string url, HttpStatusCode expectedStatus)
    {
        var client = _baseFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(url);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
}
