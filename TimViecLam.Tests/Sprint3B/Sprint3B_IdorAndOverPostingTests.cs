using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3B;

public class Sprint3B_IdorAndOverPostingTests
{
    private (JobManagementService service, DataContext context, int compAId, int compBId, int jobAId) SetupContext()
    {
        var context = TestDbHelper.CreateInMemoryContext();

        var compA = new tblCompany
        {
            Name = "Company A",
            Slug = "company-a",
            Status = "Approved",
            CreatedAt = DateTime.Now
        };
        var compB = new tblCompany
        {
            Name = "Company B",
            Slug = "company-b",
            Status = "Approved",
            CreatedAt = DateTime.Now
        };
        context.Companies.AddRange(compA, compB);

        var cat = new tblJobCategory { Name = "IT", Slug = "it", IsActive = true };
        var type = new tblJobType { Name = "Full-time", Code = "FullTime", Slug = "full-time", IsActive = true };
        context.JobCategories.Add(cat);
        context.JobTypes.Add(type);
        context.SaveChanges();

        var jobA = new tblJob
        {
            CompanyId = compA.CompanyId,
            CategoryId = cat.CategoryId,
            JobTypeId = type.JobTypeId,
            Title = "Backend Dev A",
            Slug = "backend-dev-a",
            Description = "Description A",
            ApprovalStatus = "Draft",
            ExpiresAt = DateTime.Today.AddDays(30),
            SearchText = "backend dev a company a",
            CreatedAt = DateTime.Now
        };
        context.Jobs.Add(jobA);
        context.SaveChanges();

        var sanitizer = new HtmlSanitizerService();
        var auditMock = new Mock<IAuditLogService>();
        var config = new ConfigurationBuilder().Build();
        var options = Options.Create(new ListingSettings { AutoApprove = false });

        var service = new JobManagementService(
            context,
            sanitizer,
            auditMock.Object,
            options,
            config,
            NullLogger<JobManagementService>.Instance);

        return (service, context, compA.CompanyId, compB.CompanyId, jobA.JobId);
    }

    [Fact]
    public async Task AC_S3B_1_IDOR_NTD_B_Calling_Get_Edit_Delete_Close_Duplicate_On_A_Fails()
    {
        var (service, context, compAId, compBId, jobAId) = SetupContext();

        // 1. NTD B cố lấy thông tin tin A để sửa -> Trả về null (Chống IDOR -> 404)
        var editInput = await service.GetJobInputForEditAsync(compBId, jobAId);
        Assert.Null(editInput);

        // 2. NTD B cố update tin A
        var updateModel = new JobInputModel
        {
            Title = "Hacked Title",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Hacked Desc",
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var updateResult = await service.UpdateJobAsync(compBId, 999, jobAId, updateModel);
        Assert.False(updateResult.Success);
        Assert.Equal("Không tìm thấy tin tuyển dụng.", updateResult.Message);

        // Dữ liệu tin A không hề đổi
        var originalJob = context.Jobs.Find(jobAId);
        Assert.Equal("Backend Dev A", originalJob!.Title);

        // 3. NTD B cố gửi duyệt tin A
        var sendResult = await service.SendToApprovalAsync(compBId, 999, jobAId);
        Assert.False(sendResult.Success);

        // 4. NTD B cố đóng tin A
        var closeResult = await service.CloseJobAsync(compBId, 999, jobAId);
        Assert.False(closeResult.Success);

        // 5. NTD B cố mở lại tin A
        var reopenResult = await service.ReopenJobAsync(compBId, 999, jobAId);
        Assert.False(reopenResult.Success);

        // 6. NTD B cố nhân bản tin A
        var duplicateResult = await service.DuplicateJobAsync(compBId, 999, jobAId);
        Assert.False(duplicateResult.Success);

        // 7. NTD B cố xóa tin A
        var deleteResult = await service.DeleteJobAsync(compBId, 999, jobAId);
        Assert.False(deleteResult.Success);

        // Tin A vẫn còn nguyên vẹn trong DB
        Assert.NotNull(context.Jobs.Find(jobAId));
    }

    [Fact]
    public async Task AC_S3B_2_OverPosting_CompanyId_ApprovalStatus_ViewCount_CannotBeInjected()
    {
        var (service, context, compAId, compBId, jobAId) = SetupContext();

        // Giả lập client cố tình gửi các giá trị nhạy cảm nhằm nâng quyền hoặc can thiệp trạng thái
        var model = new JobInputModel
        {
            Title = "Security Engineer",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Valid description",
            ExpiresAt = DateTime.Today.AddDays(30),
            IsDraft = false
        };

        // Gọi CreateJobAsync với CompanyId của A
        var (success, message, newJobId) = await service.CreateJobAsync(compAId, 100, model);
        Assert.True(success);
        Assert.NotNull(newJobId);

        var created = context.Jobs.Find(newJobId.Value);
        Assert.NotNull(created);
        // CompanyId phải là compAId do server truyền vào
        Assert.Equal(compAId, created.CompanyId);
        // Status do hệ thống quyết định (Pending), không nhận Published từ client
        Assert.Equal("Pending", created.ApprovalStatus);
        // ViewCount luôn là 0
        Assert.Equal(0, created.ViewCount);
        // IsFeatured luôn là false
        Assert.False(created.IsFeatured);
    }
}

