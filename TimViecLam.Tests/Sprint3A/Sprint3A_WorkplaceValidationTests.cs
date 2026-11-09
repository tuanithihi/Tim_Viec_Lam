using Microsoft.Extensions.Logging.Abstractions;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3A;

public class Sprint3A_WorkplaceValidationTests
{
    private (DataContext context, WorkplaceService service) CreateService()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        var service = new WorkplaceService(context, NullLogger<WorkplaceService>.Instance);
        return (context, service);
    }

    private async Task SeedAdministrativeData(DataContext context)
    {
        // Tỉnh 1: Hà Nội, có Quận 10 (Cầu Giấy) và Phường 100 (Dịch Vọng)
        var p1 = new tblProvince { ProvinceId = 1, Code = "01", Name = "Hà Nội", Slug = "ha-noi" };
        var d10 = new tblDistrict { DistrictId = 10, ProvinceId = 1, Code = "010", Name = "Cầu Giấy", Slug = "cau-giay" };
        var w100 = new tblWard { WardId = 100, DistrictId = 10, Code = "00100", Name = "Dịch Vọng", Slug = "dich-vong" };

        // Tỉnh 2: TP.HCM, có Quận 20 (Quận 1) và Phường 200 (Bến Nghé)
        var p2 = new tblProvince { ProvinceId = 2, Code = "79", Name = "Hồ Chí Minh", Slug = "ho-chi-minh" };
        var d20 = new tblDistrict { DistrictId = 20, ProvinceId = 2, Code = "760", Name = "Quận 1", Slug = "quan-1" };
        var w200 = new tblWard { WardId = 200, DistrictId = 20, Code = "20000", Name = "Bến Nghé", Slug = "ben-nghe" };

        context.Provinces.AddRange(p1, p2);
        context.Districts.AddRange(d10, d20);
        context.Wards.AddRange(w100, w200);

        context.Companies.Add(new tblCompany
        {
            CompanyId = 1,
            UserId = 1,
            Name = "Công ty ABC",
            Slug = "cong-ty-abc",
            Status = "Approved"
        });

        await context.SaveChangesAsync();
    }

    // ── AC-S3A.4: Tọa độ ngoài lãnh thổ Việt Nam (Lat 8–24, Lng 102–110) bị từ chối ──
    [Theory]
    [InlineData(7.9, 105.0)]   // Lat < 8.0 (Quá về phía nam)
    [InlineData(24.1, 105.0)]  // Lat > 24.0 (Quá về phía bắc)
    [InlineData(21.0, 101.9)]  // Lng < 102.0 (Quá về phía tây)
    [InlineData(21.0, 110.1)]  // Lng > 110.0 (Quá về phía đông)
    [InlineData(-10.0, 105.0)] // Âm
    public async Task AC_S3A_4_Coordinates_OutsideVietnam_AreRejected(double lat, double lng)
    {
        var (context, service) = CreateService();
        await SeedAdministrativeData(context);

        var model = new WorkplaceInputModel
        {
            Name = "Địa điểm thử nghiệm",
            Address = "Số 1 Đường ABC",
            ProvinceId = 1,
            DistrictId = 10,
            Latitude = lat,
            Longitude = lng
        };

        var result = await service.CreateWorkplaceAsync(1, model);
        Assert.False(result.Success);
        Assert.Contains("lãnh thổ Việt Nam", result.Message ?? "");
    }

    // ── AC-S3A.4: Tọa độ hợp lệ trong Việt Nam được chấp nhận ───────────────────
    [Theory]
    [InlineData(21.0285, 105.8542)] // Hà Nội
    [InlineData(10.7769, 106.7009)] // TP. Hồ Chí Minh
    [InlineData(16.0544, 108.2022)] // Đà Nẵng
    public async Task AC_S3A_4_Coordinates_InsideVietnam_AreAccepted(double lat, double lng)
    {
        var (context, service) = CreateService();
        await SeedAdministrativeData(context);

        var model = new WorkplaceInputModel
        {
            Name = "Văn phòng hợp lệ",
            Address = "Địa chỉ hợp lệ",
            ProvinceId = 1,
            DistrictId = 10,
            Latitude = lat,
            Longitude = lng
        };

        var result = await service.CreateWorkplaceAsync(1, model);
        Assert.True(result.Success);
        Assert.True(result.WorkplaceId > 0);
    }

    // ── AC-S3A.4: Quận/Huyện không thuộc Tỉnh/Thành phố bị từ chối ───────────────
    [Fact]
    public async Task AC_S3A_4_District_NotBelongingToProvince_IsRejected()
    {
        var (context, service) = CreateService();
        await SeedAdministrativeData(context);

        // Chọn Tỉnh 1 (Hà Nội) nhưng lại chọn Quận 20 (Quận 1 của TP.HCM)
        var model = new WorkplaceInputModel
        {
            Name = "Văn phòng râu ông nọ cắm cằm bà kia",
            Address = "123 Đường",
            ProvinceId = 1, // Hà Nội
            DistrictId = 20, // Quận 1 (thuộc TP.HCM)
            Latitude = 21.0,
            Longitude = 105.8
        };

        var result = await service.CreateWorkplaceAsync(1, model);
        Assert.False(result.Success);
        Assert.Contains("không thuộc Tỉnh/Thành phố", result.Message ?? "");
    }

    // ── AC-S3A.4: Phường/Xã không thuộc Quận/Huyện bị từ chối ────────────────────
    [Fact]
    public async Task AC_S3A_4_Ward_NotBelongingToDistrict_IsRejected()
    {
        var (context, service) = CreateService();
        await SeedAdministrativeData(context);

        // Chọn Tỉnh 1 (Hà Nội), Quận 10 (Cầu Giấy) nhưng lại chọn Phường 200 (Bến Nghé thuộc Quận 1)
        var model = new WorkplaceInputModel
        {
            Name = "Văn phòng phường sai lệch",
            Address = "123 Đường",
            ProvinceId = 1,
            DistrictId = 10,
            WardId = 200, // Phường Bến Nghé (thuộc Quận 1)
            Latitude = 21.0,
            Longitude = 105.8
        };

        var result = await service.CreateWorkplaceAsync(1, model);
        Assert.False(result.Success);
        Assert.Contains("không thuộc Quận/Huyện", result.Message ?? "");
    }
}
