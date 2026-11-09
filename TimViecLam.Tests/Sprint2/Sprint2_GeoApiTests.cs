using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using Xunit;

namespace TimViecLam.Tests.Sprint2;

public class Sprint2_GeoApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public Sprint2_GeoApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedSampleGeoDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        if (!context.Provinces.Any())
        {
            // Seed 63 provinces for test
            for (int i = 1; i <= 63; i++)
            {
                var province = new tblProvince
                {
                    ProvinceId = i,
                    Code = $"{i:D2}",
                    Name = i == 1 ? "Hà Nội" : (i == 2 ? "Hồ Chí Minh" : $"Tỉnh {i:D2}"),
                    FullName = i == 1 ? "Thành phố Hà Nội" : (i == 2 ? "Thành phố Hồ Chí Minh" : $"Tỉnh số {i:D2}"),
                    Slug = i == 1 ? "ha-noi" : (i == 2 ? "ho-chi-minh" : $"tinh-{i:D2}")
                };
                context.Provinces.Add(province);
            }

            // Seed sample districts for province 1
            var d1 = new tblDistrict
            {
                DistrictId = 1,
                ProvinceId = 1,
                Code = "001",
                Name = "Ba Đình",
                FullName = "Quận Ba Đình",
                Slug = "ba-dinh"
            };
            var d2 = new tblDistrict
            {
                DistrictId = 2,
                ProvinceId = 1,
                Code = "002",
                Name = "Hoàn Kiếm",
                FullName = "Quận Hoàn Kiếm",
                Slug = "hoan-kiem"
            };
            context.Districts.AddRange(d1, d2);

            // Seed sample wards for district 1
            var w1 = new tblWard
            {
                WardId = 1,
                DistrictId = 1,
                Code = "00001",
                Name = "Phúc Xá",
                FullName = "Phường Phúc Xá",
                Slug = "phuc-xa"
            };
            var w2 = new tblWard
            {
                WardId = 2,
                DistrictId = 1,
                Code = "00004",
                Name = "Trúc Bạch",
                FullName = "Phường Trúc Bạch",
                Slug = "truc-bach"
            };
            context.Wards.AddRange(w1, w2);

            await context.SaveChangesAsync();
        }
    }

    // ── AC-S2.8: GET /api/geo/provinces trả đúng 63 tỉnh & có header cache ───
    [Fact]
    public async Task AC_S2_8_GetProvinces_Returns63Items_AndCacheControlHeader()
    {
        await SeedSampleGeoDataAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/geo/provinces");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Kiểm tra header Cache-Control
        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Public);
        Assert.True(response.Headers.CacheControl.MaxAge.HasValue);

        var data = await response.Content.ReadFromJsonAsync<List<GeoItemDto>>();
        Assert.NotNull(data);
        Assert.Equal(63, data.Count);
        Assert.Contains(data, p => p.Name == "Hà Nội" && p.Slug == "ha-noi");
    }

    // ── AC-S2.8: GET /api/geo/districts tham số sai/thiếu -> 400 ─────────────
    [Theory]
    [InlineData("/api/geo/districts")]
    [InlineData("/api/geo/districts?provinceId=")]
    [InlineData("/api/geo/districts?provinceId=0")]
    [InlineData("/api/geo/districts?provinceId=-5")]
    [InlineData("/api/geo/districts?provinceId=invalid")]
    public async Task AC_S2_8_GetDistricts_MissingOrInvalidParam_Returns_400BadRequest(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── AC-S2.8: GET /api/geo/districts id hợp lệ nhưng không tồn tại -> 200 mảng rỗng
    [Fact]
    public async Task AC_S2_8_GetDistricts_NonExistentProvince_Returns_200EmptyArray()
    {
        await SeedSampleGeoDataAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/geo/districts?provinceId=99999");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<List<GeoItemDto>>();
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    // ── AC-S2.8: GET /api/geo/districts trả đúng danh sách theo cha ───────────
    [Fact]
    public async Task AC_S2_8_GetDistricts_ValidProvince_Returns_FilteredDistricts()
    {
        await SeedSampleGeoDataAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/geo/districts?provinceId=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<List<GeoItemDto>>();
        Assert.NotNull(data);
        Assert.True(data.Count >= 2);
        Assert.Contains(data, d => d.Name.Contains("Ba Đình"));
    }

    // ── AC-S2.8: GET /api/geo/wards tham số sai/thiếu -> 400 ─────────────────
    [Theory]
    [InlineData("/api/geo/wards")]
    [InlineData("/api/geo/wards?districtId=")]
    [InlineData("/api/geo/wards?districtId=0")]
    [InlineData("/api/geo/wards?districtId=-2")]
    public async Task AC_S2_8_GetWards_MissingOrInvalidParam_Returns_400BadRequest(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── AC-S2.8: GET /api/geo/wards id hợp lệ nhưng không tồn tại -> 200 mảng rỗng
    [Fact]
    public async Task AC_S2_8_GetWards_NonExistentDistrict_Returns_200EmptyArray()
    {
        await SeedSampleGeoDataAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/geo/wards?districtId=88888");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<List<GeoItemDto>>();
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    // ── AC-S2.8: GET /api/geo/wards trả đúng danh sách theo cha ───────────────
    [Fact]
    public async Task AC_S2_8_GetWards_ValidDistrict_Returns_FilteredWards()
    {
        await SeedSampleGeoDataAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/geo/wards?districtId=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<List<GeoItemDto>>();
        Assert.NotNull(data);
        Assert.True(data.Count >= 2);
        Assert.Contains(data, w => w.Name.Contains("Phúc Xá"));
    }
}
