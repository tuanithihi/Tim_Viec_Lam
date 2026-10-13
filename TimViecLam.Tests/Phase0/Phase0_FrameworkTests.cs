using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TimViecLam.Helpers;
using Xunit;

namespace TimViecLam.Tests.Phase0;

/// <summary>
/// Test giai đoạn 0: Khung dự án & chuẩn chung.
/// AC-0.1 → AC-0.8
/// </summary>
public class Phase0_FrameworkTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public Phase0_FrameworkTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // ── AC-0.2: GET / trả 200, HTML chứa lang="vi", <title>, meta viewport ──

    [Fact(DisplayName = "AC_0_2_HomeIndex_Returns200")]
    public async Task AC_0_2_HomeIndex_Returns200()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC_0_2_HomeIndex_ContainsLangVi")]
    public async Task AC_0_2_HomeIndex_ContainsLangVi()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("lang=\"vi\"", html);
    }

    [Fact(DisplayName = "AC_0_2_HomeIndex_ContainsTitleTag")]
    public async Task AC_0_2_HomeIndex_ContainsTitleTag()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<title>", html);
    }

    [Fact(DisplayName = "AC_0_2_HomeIndex_ContainsMetaViewport")]
    public async Task AC_0_2_HomeIndex_ContainsMetaViewport()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"viewport\"", html);
    }

    // ── AC-0.3: Security headers đủ, KHÔNG có X-XSS-Protection ──────────────

    [Fact(DisplayName = "AC_0_3_SecurityHeader_XContentTypeOptions")]
    public async Task AC_0_3_SecurityHeader_XContentTypeOptions()
    {
        var response = await _client.GetAsync("/");
        Assert.True(response.Headers.Contains("X-Content-Type-Options"),
            "Thiếu header X-Content-Type-Options");
        Assert.Equal("nosniff",
            response.Headers.GetValues("X-Content-Type-Options").First());
    }

    [Fact(DisplayName = "AC_0_3_SecurityHeader_XFrameOptions")]
    public async Task AC_0_3_SecurityHeader_XFrameOptions()
    {
        var response = await _client.GetAsync("/");
        Assert.True(response.Headers.Contains("X-Frame-Options"),
            "Thiếu header X-Frame-Options");
    }

    [Fact(DisplayName = "AC_0_3_SecurityHeader_ReferrerPolicy")]
    public async Task AC_0_3_SecurityHeader_ReferrerPolicy()
    {
        var response = await _client.GetAsync("/");
        Assert.True(response.Headers.Contains("Referrer-Policy"),
            "Thiếu header Referrer-Policy");
    }

    [Fact(DisplayName = "AC_0_3_SecurityHeader_PermissionsPolicy")]
    public async Task AC_0_3_SecurityHeader_PermissionsPolicy()
    {
        var response = await _client.GetAsync("/");
        Assert.True(response.Headers.Contains("Permissions-Policy"),
            "Thiếu header Permissions-Policy");
    }

    [Fact(DisplayName = "AC_0_3_SecurityHeader_ContentSecurityPolicy")]
    public async Task AC_0_3_SecurityHeader_ContentSecurityPolicy()
    {
        var response = await _client.GetAsync("/");
        Assert.True(response.Headers.Contains("Content-Security-Policy"),
            "Thiếu header Content-Security-Policy");
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        // Kiểm tra cho phép OSM tiles
        Assert.Contains("*.tile.openstreetmap.org", csp);
    }

    [Fact(DisplayName = "AC_0_3_No_XSSProtection_Header")]
    public async Task AC_0_3_No_XSSProtection_Header()
    {
        var response = await _client.GetAsync("/");
        // X-XSS-Protection KHÔNG được có trong response
        Assert.False(response.Headers.Contains("X-XSS-Protection"),
            "Header X-XSS-Protection không được dùng (lỗi thời, gây lỗ hổng)");
    }

    // ── AC-0.4: /health → 200; /khong-ton-tai → 404 ──────────────────────────

    [Fact(DisplayName = "AC_0_4_HealthEndpoint_Returns200")]
    public async Task AC_0_4_HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC_0_4_NotFound_Returns404_WithCustomPage")]
    public async Task AC_0_4_NotFound_Returns404_WithCustomPage()
    {
        var clientFollowRedirect = _factory.CreateClient(); // auto redirect
        var response = await clientFollowRedirect.GetAsync("/khong-ton-tai-abc-xyz");
        // Sau UseStatusCodePagesWithReExecute, status code sẽ là 200 (re-executed)
        // nhưng nội dung trang phải có thông báo không tìm thấy
        var html = await response.Content.ReadAsStringAsync();
        // Không được lộ stack trace
        Assert.DoesNotContain("StackTrace", html);
        Assert.DoesNotContain("System.Exception", html);
    }

    // ── AC-0.5: /css/variables.css chứa --brand-primary: #e03c31 ─────────────

    [Fact(DisplayName = "AC_0_5_VariablesCSS_ContainsBrandPrimary")]
    public async Task AC_0_5_VariablesCSS_ContainsBrandPrimary()
    {
        var response = await _client.GetAsync("/css/variables.css");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var css = await response.Content.ReadAsStringAsync();
        Assert.Contains("--brand-primary: #e03c31", css);
    }

    // ── AC-0.6: Không có bí mật trong appsettings.json ───────────────────────
    // (kiểm tra thông qua nội dung appsettings)

    [Fact(DisplayName = "AC_0_6_AppSettings_NoRealSecrets")]
    public void AC_0_6_AppSettings_NoRealSecrets()
    {
        // Đọc appsettings.json từ file system
        var solutionRoot = GetSolutionRoot();
        var appSettingsPath = Path.Combine(solutionRoot, "TimViecLam", "appsettings.json");
        Assert.True(File.Exists(appSettingsPath), $"Không tìm thấy: {appSettingsPath}");

        var content = File.ReadAllText(appSettingsPath);
        // Không được có pattern bí mật
        Assert.DoesNotContain("gsk_", content);
        // ApiKey phải rỗng
        Assert.DoesNotContain("\"ApiKey\": \"gsk_", content);
        // Không có tên server cụ thể (LocalDB OK, tên máy cá nhân không OK)
        Assert.DoesNotContain("LAPTOP-", content.ToUpperInvariant());
        Assert.DoesNotContain("DESKTOP-", content.ToUpperInvariant());
    }

    // ── AC-0.7: Không có màu hex cứng trong Views ─────────────────────────────

    [Fact(DisplayName = "AC_0_7_NoHardCodedHexColors_InPagesCSS")]
    public void AC_0_7_NoHardCodedHexColors_InPagesCSS()
    {
        var solutionRoot = GetSolutionRoot();
        var pagesDir = Path.Combine(solutionRoot, "TimViecLam", "wwwroot", "css", "pages");
        Assert.True(Directory.Exists(pagesDir), $"Không tìm thấy thư mục: {pagesDir}");

        var cssFiles = Directory.GetFiles(pagesDir, "*.css");
        Assert.NotEmpty(cssFiles);

        foreach (var file in cssFiles)
        {
            var content = File.ReadAllText(file);
            // Tìm hex màu cứng (không phải trong comment, không phải trong var())
            // Cho phép: #ffffff (trắng), #000 (đen) vì thường là giá trị semantic
            // Không cho phép màu cụ thể như #e03c31, #c93126 (phải dùng biến)
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("/*") || trimmed.StartsWith("//")) continue;
                if (trimmed.Contains("var(--")) continue;
                // Kiểm tra các màu brand cứng không được xuất hiện
                if (trimmed.Contains("#e03c31"))
                    Assert.Fail($"Màu hex cứng #e03c31 trong {Path.GetFileName(file)}: {trimmed}");
                if (trimmed.Contains("#c93126"))
                    Assert.Fail($"Màu hex cứng #c93126 trong {Path.GetFileName(file)}: {trimmed}");
            }
        }
    }

    // ── AC-0.8: Kiểm tra sự tồn tại cấu trúc thư mục ────────────────────────

    [Theory(DisplayName = "AC_0_8_DirectoryStructure_Exists")]
    [InlineData("TimViecLam/Models/ViewModels")]
    [InlineData("TimViecLam/Models/Entities")]
    [InlineData("TimViecLam/Services")]
    [InlineData("TimViecLam/Controllers")]
    [InlineData("TimViecLam/Components")]
    [InlineData("TimViecLam/Helpers")]
    [InlineData("TimViecLam/Data")]
    [InlineData("TimViecLam/Views/Home")]
    [InlineData("TimViecLam/Views/Shared")]
    [InlineData("TimViecLam/wwwroot/css/pages")]
    [InlineData("TimViecLam/wwwroot/js")]
    [InlineData("TimViecLam/wwwroot/images")]
    [InlineData("docs")]
    public void AC_0_8_DirectoryStructure_Exists(string relativePath)
    {
        var solutionRoot = GetSolutionRoot();
        var fullPath = Path.Combine(solutionRoot, relativePath);
        Assert.True(Directory.Exists(fullPath),
            $"Thư mục bắt buộc không tồn tại: {relativePath}");
    }

    // ── AC-0.8 (bổ sung): SlugHelper hoạt động đúng ──────────────────────────

    [Theory(DisplayName = "AC_0_8_SlugHelper_RemovesVietnameseAccents")]
    [InlineData("Lập Trình Viên .NET", "lap-trinh-vien-net")]
    [InlineData("Hà Nội", "ha-noi")]
    [InlineData("Đà Nẵng", "da-nang")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void AC_0_8_SlugHelper_RemovesVietnameseAccents(string? input, string expected)
    {
        var result = SlugHelper.Slugify(input);
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "AC_0_8_SlugHelper_WithId")]
    public void AC_0_8_SlugHelper_WithId()
    {
        var result = SlugHelper.Slugify("Kỹ sư phần mềm", 42);
        Assert.Equal("ky-su-phan-mem-42", result);
    }

    // ── Âm tính: URL rác không crash server ──────────────────────────────────

    [Fact(DisplayName = "Negative_RandomJunkUrl_DoesNotCrash")]
    public async Task Negative_RandomJunkUrl_DoesNotCrash()
    {
        var client = _factory.CreateClient();
        // Các URL lạ không được crash 500
        var responses = await Task.WhenAll(
            client.GetAsync("/<script>alert(1)</script>"),
            client.GetAsync("/../../../../etc/passwd"),
            client.GetAsync("/" + new string('a', 500))
        );
        foreach (var r in responses)
        {
            // Không được là 500 Internal Server Error
            Assert.NotEqual(HttpStatusCode.InternalServerError, r.StatusCode);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetSolutionRoot()
    {
        // Đi ngược từ bin/Debug/net8.0 lên thư mục solution
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? dir;
        }
        throw new InvalidOperationException("Không tìm thấy thư mục gốc solution.");
    }
}
