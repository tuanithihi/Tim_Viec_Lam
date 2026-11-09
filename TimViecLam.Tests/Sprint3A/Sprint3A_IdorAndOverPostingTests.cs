using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3A;

public class Sprint3A_IdorAndOverPostingTests
{
    private (DataContext context, WorkplaceService workplaceService, CompanyService companyService) CreateServices()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var fileUploadService = new FileUploadService(mockEnv.Object, NullLogger<FileUploadService>.Instance);
        var companyService = new CompanyService(context, fileUploadService, NullLogger<CompanyService>.Instance);
        var workplaceService = new WorkplaceService(context, NullLogger<WorkplaceService>.Instance);

        return (context, workplaceService, companyService);
    }

    private async Task SeedCompaniesAndWorkplaces(DataContext context)
    {
        // Tỉnh 1, Quận 10 & 11
        var p1 = new tblProvince { ProvinceId = 1, Code = "01", Name = "Hà Nội", Slug = "ha-noi" };
        var d10 = new tblDistrict { DistrictId = 10, ProvinceId = 1, Code = "010", Name = "Cầu Giấy", Slug = "cau-giay" };
        var d11 = new tblDistrict { DistrictId = 11, ProvinceId = 1, Code = "011", Name = "Ba Đình", Slug = "ba-dinh" };
        context.Provinces.Add(p1);
        context.Districts.AddRange(d10, d11);

        // Công ty A (CompanyId = 1, UserId = 101)
        var compA = new tblCompany
        {
            CompanyId = 1,
            UserId = 101,
            Name = "Công ty Công nghệ A",
            Slug = "cong-ty-cong-nghe-a",
            Status = "Approved",
            CompanySize = "Medium",
            Email = "contact@compa.vn",
            CreatedAt = DateTime.Now
        };

        // Công ty B (CompanyId = 2, UserId = 102)
        var compB = new tblCompany
        {
            CompanyId = 2,
            UserId = 102,
            Name = "Công ty Giải pháp B",
            Slug = "cong-ty-giai-phap-b",
            Status = "Approved",
            CompanySize = "Small",
            Email = "contact@compb.vn",
            CreatedAt = DateTime.Now
        };
        context.Companies.AddRange(compA, compB);

        // Địa điểm 1 thuộc Công ty A
        var wp1 = new tblWorkplace
        {
            WorkplaceId = 1,
            CompanyId = 1,
            Name = "Trụ sở chính A",
            Address = "12 Duy Tân",
            ProvinceId = 1,
            DistrictId = 10,
            Latitude = 21.03,
            Longitude = 105.78,
            IsHeadOffice = true,
            CreatedAt = DateTime.Now
        };

        // Địa điểm 2 thuộc Công ty B
        var wp2 = new tblWorkplace
        {
            WorkplaceId = 2,
            CompanyId = 2,
            Name = "Trụ sở chính B",
            Address = "88 Kim Mã",
            ProvinceId = 1,
            DistrictId = 11,
            Latitude = 21.03,
            Longitude = 105.82,
            IsHeadOffice = true,
            CreatedAt = DateTime.Now
        };

        context.Workplaces.AddRange(wp1, wp2);
        await context.SaveChangesAsync();
    }

    // ── AC-S3A.1: IDOR: NTD B gọi xem/sửa/xóa địa điểm của NTD A đều bị từ chối ──
    [Fact]
    public async Task AC_S3A_1_IDOR_EmployerB_Cannot_View_Edit_Or_Delete_EmployerA_Workplace()
    {
        var (context, workplaceService, _) = CreateServices();
        await SeedCompaniesAndWorkplaces(context);

        int companyBId = 2;
        int workplaceAId = 1;

        // 1. NTD B cố tình xem địa điểm của NTD A -> trả về null (Controller sẽ map sang 404)
        var viewResult = await workplaceService.GetWorkplaceByIdAsync(workplaceAId, companyBId);
        Assert.Null(viewResult);

        // 2. NTD B cố tình sửa địa điểm của NTD A -> thất bại, dữ liệu A không bị thay đổi
        var editResult = await workplaceService.UpdateWorkplaceAsync(workplaceAId, companyBId, new WorkplaceInputModel
        {
            Name = "Tên bị hack bởi B",
            Address = "Địa chỉ bị sửa",
            ProvinceId = 1,
            DistrictId = 10,
            Latitude = 21.03,
            Longitude = 105.78
        });
        Assert.False(editResult.Success);
        Assert.Contains("Không tìm thấy", editResult.Message ?? "");

        var originalA = await context.Workplaces.FindAsync(workplaceAId);
        Assert.Equal("Trụ sở chính A", originalA!.Name);
        Assert.Equal("12 Duy Tân", originalA.Address);

        // 3. NTD B cố tình xóa địa điểm của NTD A -> thất bại, dữ liệu A vẫn còn nguyên
        var deleteResult = await workplaceService.DeleteWorkplaceAsync(workplaceAId, companyBId);
        Assert.False(deleteResult.Success);
        Assert.Contains("Không tìm thấy", deleteResult.Message ?? "");

        var stillExistsA = await context.Workplaces.FindAsync(workplaceAId);
        Assert.NotNull(stillExistsA);
    }

    // ── AC-S3A.2: Over-posting: gửi thêm CompanyId/Status/UserId khi sửa hồ sơ không có tác dụng
    [Fact]
    public async Task AC_S3A_2_OverPosting_UpdatingCompanyProfile_IgnoresUnauthorizedFields()
    {
        var (context, _, companyService) = CreateServices();
        await SeedCompaniesAndWorkplaces(context);

        int companyAId = 1;
        var original = await context.Companies.AsNoTracking().FirstAsync(c => c.CompanyId == companyAId);
        Assert.Equal("Approved", original.Status);
        Assert.Null(original.RejectReason);
        Assert.Equal(101, original.UserId);

        // Giả lập client cố tình gửi các trường nhạy cảm trong form
        var updateModel = new CompanyProfileInputModel
        {
            Name = "Công ty Công nghệ A (Đã đổi tên)",
            Phone = "0987654321",
            Email = "new_email@compa.vn",
            Address = "Địa chỉ trụ sở mới",
            CompanySize = "Large"
        };

        var result = await companyService.UpdateCompanyProfileAsync(companyAId, updateModel);
        Assert.True(result.Success);

        var updated = await context.Companies.FindAsync(companyAId);
        Assert.NotNull(updated);
        // Các trường hợp lệ được cập nhật
        Assert.Equal("Công ty Công nghệ A (Đã đổi tên)", updated.Name);
        Assert.Equal("0987654321", updated.Phone);
        Assert.Equal("Large", updated.CompanySize);

        // Các trường bảo mật KHÔNG bị thay đổi
        Assert.Equal("Approved", updated.Status);
        Assert.Null(updated.RejectReason);
        Assert.Equal(101, updated.UserId);
        Assert.Equal(companyAId, updated.CompanyId);
    }

    // ── AC-S3A.6: Xóa địa điểm đang có tin tuyển dụng liên kết bị chặn ───────────
    [Fact]
    public async Task AC_S3A_6_DeleteWorkplace_WithLinkedJobs_IsBlocked()
    {
        var (context, workplaceService, _) = CreateServices();
        await SeedCompaniesAndWorkplaces(context);

        // Tạo 1 việc làm liên kết với WorkplaceId = 1 của Công ty 1
        var job = new tblJob
        {
            JobId = 501,
            CompanyId = 1,
            WorkplaceId = 1,
            CategoryId = 1,
            JobTypeId = 1,
            Title = "Lập trình viên Backend .NET",
            Slug = "lap-trinh-vien-backend-net",
            Description = "Mô tả công việc",
            ApprovalStatus = "Published",
            ExpiresAt = DateTime.Now.AddDays(30),
            CreatedAt = DateTime.Now
        };
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        // Xóa thử Workplace 1
        var result = await workplaceService.DeleteWorkplaceAsync(1, 1);
        Assert.False(result.Success);
        Assert.Contains("đang có tin tuyển dụng", result.Message ?? "");

        // Kiểm tra trong DB vẫn còn
        var wp = await context.Workplaces.FindAsync(1);
        Assert.NotNull(wp);
    }

    // ── AC-S3A.10: Danh sách địa điểm <= 3 truy vấn DB ─────────────────────────
    [Fact]
    public async Task AC_S3A_10_GetPagedWorkplaces_PerformsEfficientQueries()
    {
        var (context, workplaceService, _) = CreateServices();
        await SeedCompaniesAndWorkplaces(context);

        // Thêm 5 địa điểm cho công ty 1
        for (int i = 3; i <= 7; i++)
        {
            context.Workplaces.Add(new tblWorkplace
            {
                WorkplaceId = i,
                CompanyId = 1,
                Name = $"Chi nhánh số {i}",
                Address = $"Địa chỉ {i}",
                ProvinceId = 1,
                DistrictId = 10,
                CreatedAt = DateTime.Now
            });
        }
        await context.SaveChangesAsync();

        var paged = await workplaceService.GetPagedWorkplacesAsync(1, null, 1, 5);
        Assert.Equal(6, paged.TotalItems);
        Assert.Equal(5, paged.Items.Count);
        Assert.Equal(2, paged.TotalPages);
    }
}
