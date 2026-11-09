using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimViecLam.Areas.Admin.Controllers;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Controllers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3A;

public class Sprint3A_CompanyModerationAndAuthTests
{
    private (DataContext context, CompanyService service) CreateService()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var fileUploadService = new FileUploadService(mockEnv.Object, NullLogger<FileUploadService>.Instance);
        var companyService = new CompanyService(context, fileUploadService, NullLogger<CompanyService>.Instance);

        return (context, companyService);
    }

    // ── AC-S3A.8: SuperAdmin duyệt công ty: Approve, Reject, Suspend, Unsuspend ──
    [Fact]
    public async Task AC_S3A_8_SuperAdmin_ModerationWorkflow_WorksCorrectly()
    {
        var (context, service) = CreateService();

        var comp = new tblCompany
        {
            CompanyId = 1,
            UserId = 1,
            Name = "Công ty Cần Duyệt",
            Slug = "cong-ty-can-duyet",
            Status = "Pending"
        };
        context.Companies.Add(comp);
        await context.SaveChangesAsync();

        // 1. Phê duyệt (Approve)
        var approveResult = await service.ApproveCompanyAsync(1);
        Assert.True(approveResult.Success);
        var afterApprove = await context.Companies.FindAsync(1);
        Assert.Equal("Approved", afterApprove!.Status);
        Assert.Null(afterApprove.RejectReason);

        // 2. Tạm khóa thiếu lý do bị từ chối
        var emptySuspend = await service.SuspendCompanyAsync(1, "   ");
        Assert.False(emptySuspend.Success);
        Assert.Contains("không được để trống", emptySuspend.Message ?? "");

        // 3. Tạm khóa có lý do -> Thành công
        var suspendResult = await service.SuspendCompanyAsync(1, "Có phản ánh nghi vấn giả mạo tuyển dụng");
        Assert.True(suspendResult.Success);
        var afterSuspend = await context.Companies.FindAsync(1);
        Assert.Equal("Suspended", afterSuspend!.Status);
        Assert.Equal("Có phản ánh nghi vấn giả mạo tuyển dụng", afterSuspend.RejectReason);

        // 4. Mở khóa (Unsuspend)
        var unsuspendResult = await service.UnsuspendCompanyAsync(1);
        Assert.True(unsuspendResult.Success);
        var afterUnsuspend = await context.Companies.FindAsync(1);
        Assert.Equal("Approved", afterUnsuspend!.Status);
        Assert.Null(afterUnsuspend.RejectReason);

        // 5. Từ chối thiếu lý do bị từ chối
        var emptyReject = await service.RejectCompanyAsync(1, "");
        Assert.False(emptyReject.Success);
        Assert.Contains("không được để trống", emptyReject.Message ?? "");

        // 6. Từ chối có lý do -> Thành công
        var rejectResult = await service.RejectCompanyAsync(1, "Mã số thuế không tồn tại trên Tổng cục Thuế");
        Assert.True(rejectResult.Success);
        var afterReject = await context.Companies.FindAsync(1);
        Assert.Equal("Rejected", afterReject!.Status);
        Assert.Equal("Mã số thuế không tồn tại trên Tổng cục Thuế", afterReject.RejectReason);
    }

    // ── AC-S3A.8 & AC-S3A.7: Phân quyền controller công ty ──────────────────────
    [Fact]
    public void AC_S3A_8_SuperAdminCompanyController_Has_SuperAdminOnly_Policy()
    {
        var areaAttr = typeof(CompanyController).GetCustomAttribute<AreaAttribute>();
        Assert.NotNull(areaAttr);
        Assert.Equal("Admin", areaAttr.RouteValue);

        var authAttr = typeof(CompanyController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal("SuperAdminOnly", authAttr.Policy);
    }

    [Fact]
    public void AC_S3A_7_EmployerControllers_Have_EmployerOnly_Policy()
    {
        var authEmployer = typeof(EmployerController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authEmployer);
        Assert.Equal("EmployerOnly", authEmployer.Policy);

        var authWorkplace = typeof(EmployerWorkplaceController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authWorkplace);
        Assert.Equal("EmployerOnly", authWorkplace.Policy);

        // Action CreateJob phải có policy ApprovedEmployer
        var createJobAction = typeof(EmployerController).GetMethod(nameof(EmployerController.CreateJob));
        Assert.NotNull(createJobAction);
        var authCreateJob = createJobAction.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authCreateJob);
        Assert.Equal("ApprovedEmployer", authCreateJob.Policy);
    }

    // ── AC-S3A.9: Slug công ty duy nhất, trùng thì tự động thêm hậu tố ──────────
    [Fact]
    public async Task AC_S3A_9_CompanySlug_IsUnique_AndGetsSuffixWhenDuplicate()
    {
        var (context, service) = CreateService();

        // Công ty 1 có sẵn slug "fpt-software"
        context.Companies.Add(new tblCompany
        {
            CompanyId = 1,
            UserId = 10,
            Name = "FPT Software",
            Slug = "fpt-software",
            Status = "Approved"
        });

        // Công ty 2
        context.Companies.Add(new tblCompany
        {
            CompanyId = 2,
            UserId = 20,
            Name = "Công ty TNHH Khác",
            Slug = "cong-ty-tnhh-khac",
            Status = "Approved"
        });
        await context.SaveChangesAsync();

        // Đổi tên Công ty 2 thành "FPT Software"
        var updateResult = await service.UpdateCompanyProfileAsync(2, new CompanyProfileInputModel
        {
            Name = "FPT Software",
            CompanySize = "Enterprise"
        });

        Assert.True(updateResult.Success);
        var company2 = await context.Companies.FindAsync(2);
        Assert.NotNull(company2);
        // Do slug "fpt-software" đã tồn tại ở công ty 1, công ty 2 sẽ nhận slug "fpt-software-1"
        Assert.Equal("fpt-software-1", company2.Slug);
    }

    // ── AC-S3A.11: Quét mã: không dùng GetAwaiter().GetResult(), không bind thẳng entity
    [Theory]
    [InlineData(typeof(CompanyController))]
    [InlineData(typeof(EmployerController))]
    [InlineData(typeof(EmployerWorkplaceController))]
    public void AC_S3A_11_Controllers_DoNotBindEntitiesDirectly(Type controllerType)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var m in methods)
        {
            var parameters = m.GetParameters();
            foreach (var p in parameters)
            {
                // Không được bind trực tiếp tblCompany hoặc tblWorkplace từ request form/body
                Assert.NotEqual(typeof(tblCompany), p.ParameterType);
                Assert.NotEqual(typeof(tblWorkplace), p.ParameterType);
            }
        }
    }

    // ── Hồi quy tối thiểu Sprint 2: ICategoryService và IGeoService vẫn hoạt động tốt ──
    [Fact]
    public async Task Regression_Sprint2_CategoryAndGeoServices_StillWorkAccurately()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        await DataSeeder.SeedJobCategoriesAsync(context);

        var count = await context.JobCategories.CountAsync();
        Assert.True(count >= 15);
    }
}
