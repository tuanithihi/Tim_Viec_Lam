using System.Net;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimViecLam.Areas.Admin.Controllers;
using Xunit;

namespace TimViecLam.Tests.Sprint2;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string CurrentRole = "Candidate";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrEmpty(CurrentRole))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "100"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, CurrentRole)
        };

        if (CurrentRole == "Employer")
        {
            claims.Add(new Claim("CompanyId", "1"));
        }

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class Sprint2_AuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public Sprint2_AuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── AC-S2.1: Reflection scan trên các controller danh mục ─────────────────
    [Theory]
    [InlineData(typeof(CategoryController))]
    [InlineData(typeof(JobTypeController))]
    [InlineData(typeof(SkillController))]
    [InlineData(typeof(BenefitController))]
    [InlineData(typeof(LocationController))]
    public void AC_S2_1_Controllers_Have_AdminArea_And_SuperAdminOnly_Policy(Type controllerType)
    {
        var areaAttr = controllerType.GetCustomAttribute<AreaAttribute>();
        Assert.NotNull(areaAttr);
        Assert.Equal("Admin", areaAttr.RouteValue);

        var authAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal("SuperAdminOnly", authAttr.Policy);
    }

    // ── AC-S2.1: Khách (chưa đăng nhập) bị chuyển hướng 302 đến trang đăng nhập
    [Theory]
    [InlineData("/Admin/Category")]
    [InlineData("/Admin/JobType")]
    [InlineData("/Admin/Skill")]
    [InlineData("/Admin/Benefit")]
    [InlineData("/Admin/Location")]
    public async Task AC_S2_1_UnauthenticatedGuest_RedirectsToLogin_302(string url)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/dang-nhap", response.Headers.Location?.ToString() ?? "");
    }

    // ── AC-S2.1: Candidate & Employer bị từ chối 403 Forbidden ────────────────
    [Theory]
    [InlineData("Candidate")]
    [InlineData("Employer")]
    public async Task AC_S2_1_NonSuperAdmin_Returns_Forbidden_403(string role)
    {
        TestAuthHandler.CurrentRole = role;

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin/Category");
        // ASP.NET Core cookie authentication redirects to AccessDenied (403 redirect) or returns 403
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location?.ToString().Contains("403") == true) ||
            (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location?.ToString().Contains("AccessDenied") == true),
            $"Expected 403 Forbidden or redirect to AccessDenied/403, but received {response.StatusCode} to {response.Headers.Location}");
    }

    // ── AC-S2.1: SuperAdmin truy cập thành công 200 OK ────────────────────────
    [Fact]
    public async Task AC_S2_1_SuperAdmin_CanAccessAdminCategories_200()
    {
        TestAuthHandler.CurrentRole = "SuperAdmin";

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin/Category");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── AC-S2.10: POST thiếu Antiforgery token bị từ chối 400 Bad Request ──────
    [Fact]
    public async Task AC_S2_10_PostWithoutAntiforgeryToken_Returns_400BadRequest()
    {
        TestAuthHandler.CurrentRole = "SuperAdmin";

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", "Ngành Thử Nghiệm" },
            { "Description", "Mô tả" },
            { "SortOrder", "1" },
            { "IsActive", "true" }
        });

        var response = await client.PostAsync("/Admin/Category/Create", formContent);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
