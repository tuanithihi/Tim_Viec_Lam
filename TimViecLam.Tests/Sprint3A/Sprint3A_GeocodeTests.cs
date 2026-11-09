using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3A;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public int RequestCount { get; private set; }
    public HttpRequestMessage? LastRequest { get; private set; }
    public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        LastRequest = request;

        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }

        if (ResponseFactory != null)
        {
            return ResponseFactory(request);
        }

        string jsonResponse = "[{\"display_name\":\"Hà Nội, Việt Nam\",\"lat\":\"21.028511\",\"lon\":\"105.854444\"}]";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };
    }
}

public class Sprint3A_GeocodeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public Sprint3A_GeocodeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── AC-S3A.5: Endpoint /api/geo/geocode yêu cầu đăng nhập (Khách -> 401/302) ──
    [Fact]
    public async Task AC_S3A_5_GeocodeEndpoint_Unauthenticated_Returns_401_Or_302()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/geo/geocode?q=Hanoi");
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 Unauthorized or 302 Redirect, but got {response.StatusCode}");
    }

    // ── AC-S3A.5: Gọi lần 2 cùng truy vấn không phát sinh request ra ngoài (Cache) ──
    [Fact]
    public async Task AC_S3A_5_SecondCall_SameQuery_UsesCache_DoesNotMakeExternalRequest()
    {
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var geocodeService = new NominatimGeocodeService(httpClient, cache, NullLogger<NominatimGeocodeService>.Instance);

        // Lần gọi 1
        var result1 = await geocodeService.SearchAsync("Hà Nội");
        Assert.True(result1.Success);
        Assert.Equal(1, mockHandler.RequestCount);
        Assert.Single(result1.Results);
        Assert.Equal(21.028511, result1.Results[0].Latitude);

        // Lần gọi 2 (cùng truy vấn, không phân biệt hoa thường)
        var result2 = await geocodeService.SearchAsync("  hà nội  ");
        Assert.True(result2.Success);
        // RequestCount vẫn là 1 vì đã lấy từ Cache!
        Assert.Equal(1, mockHandler.RequestCount);
        Assert.Single(result2.Results);
        Assert.Equal(result1.Results[0].DisplayName, result2.Results[0].DisplayName);
    }

    // ── AC-S3A.5: Gửi User-Agent định danh theo đúng quy định của Nominatim ──────
    [Fact]
    public async Task AC_S3A_5_NominatimRequest_SendsCustomUserAgent()
    {
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var geocodeService = new NominatimGeocodeService(httpClient, cache, NullLogger<NominatimGeocodeService>.Instance);

        await geocodeService.SearchAsync("Đà Nẵng");

        Assert.NotNull(mockHandler.LastRequest);
        var userAgent = mockHandler.LastRequest.Headers.UserAgent.ToString();
        Assert.Contains("TimViecLam-JobPortal/1.0", userAgent);
    }

    // ── AC-S3A.5: Timeout sau 5 giây -> phản hồi lỗi thân thiện ────────────────
    [Fact]
    public async Task AC_S3A_5_Timeout_Returns_FriendlyErrorMessage()
    {
        var mockHandler = new MockHttpMessageHandler
        {
            // Giả lập server bên ngoài bị treo lâu hơn 5 giây
            Delay = TimeSpan.FromSeconds(6)
        };
        var httpClient = new HttpClient(mockHandler)
        {
            Timeout = TimeSpan.FromSeconds(2) // Ép timeout sớm trong test để chạy nhanh
        };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var geocodeService = new NominatimGeocodeService(httpClient, cache, NullLogger<NominatimGeocodeService>.Instance);

        var result = await geocodeService.SearchAsync("Địa chỉ bị treo timeout");

        Assert.False(result.Success);
        Assert.Contains("thời gian", result.Message ?? "");
    }
}
