using System.Net;
using TimViecLam.Tests;
using Xunit;

namespace TimViecLam.Tests.Sprint3B;

public class Sprint3B_HttpEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public Sprint3B_HttpEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AC_S3B_10_AdminJobModeration_AnonymousUser_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var res = await client.GetAsync("/admin/duyet-tin");
        // Người dùng chưa đăng nhập -> 302 Redirect về trang đăng nhập
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.Contains("/dang-nhap", res.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AC_S3B_6_EmployerJobs_AnonymousUser_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var res = await client.GetAsync("/nha-tuyen-dung/tin-tuyen-dung");
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.Contains("/dang-nhap", res.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AC_S3B_6_EmployerCreateJob_AnonymousUser_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var res = await client.GetAsync("/nha-tuyen-dung/tin-tuyen-dung/them");
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.Contains("/dang-nhap", res.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AC_S3B_POST_WithoutAntiforgeryToken_Rejected()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // POST không có AntiForgeryToken -> Bị từ chối (400 Bad Request hoặc 302 về Login nếu qua Auth trước)
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Title", "Hacked Post" }
        });

        var res = await client.PostAsync("/nha-tuyen-dung/tin-tuyen-dung/them", content);
        // Do AutoValidateAntiforgeryToken và Authorize:
        Assert.True(res.StatusCode == HttpStatusCode.BadRequest || res.StatusCode == HttpStatusCode.Redirect);
    }
}

