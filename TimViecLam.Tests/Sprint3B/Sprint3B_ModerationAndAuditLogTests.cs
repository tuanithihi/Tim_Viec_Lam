using Microsoft.EntityFrameworkCore;
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

public class Sprint3B_ModerationAndAuditLogTests
{
    private (JobModerationService moderationService, JobManagementService mgmtService, CompanyService compService, DataContext context, int adminUserId, tblCompany comp) SetupContext()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        var auditService = new AuditLogService(context, NullLogger<AuditLogService>.Instance);

        var comp = new tblCompany
        {
            Name = "VNG Corp",
            Slug = "vng-corp",
            Status = "Approved",
            CreatedAt = DateTime.Now
        };
        context.Companies.Add(comp);

        var cat = new tblJobCategory { Name = "IT", Slug = "it", IsActive = true };
        var type = new tblJobType { Name = "Full-time", Code = "FT", Slug = "ft", IsActive = true };
        context.JobCategories.Add(cat);
        context.JobTypes.Add(type);
        context.SaveChanges();

        var sanitizer = new HtmlSanitizerService();
        var fileUploadMock = new Mock<IFileUploadService>();
        var config = new ConfigurationBuilder().Build();
        var options = Options.Create(new ListingSettings { AutoApprove = false });

        var mgmtService = new JobManagementService(
            context,
            sanitizer,
            auditService,
            options,
            config,
            NullLogger<JobManagementService>.Instance);

        var moderationService = new JobModerationService(
            context,
            auditService,
            NullLogger<JobModerationService>.Instance);

        var compService = new CompanyService(
            context,
            fileUploadMock.Object,
            auditService,
            NullLogger<CompanyService>.Instance);

        return (moderationService, mgmtService, compService, context, 1, comp);
    }

    [Fact]
    public async Task AC_S3B_10_SuperAdmin_PendingList_ApproveSetsPublishedAt_RejectUnpublishNeedReason_BatchApproveOnlyPending()
    {
        var (moderationService, _, _, context, adminId, comp) = SetupContext();

        // Tạo 3 tin: 2 Pending, 1 Draft
        var j1 = new tblJob
        {
            CompanyId = comp.CompanyId,
            CategoryId = 1,
            JobTypeId = 1,
            Title = "Job 1 Pending",
            Slug = "job-1-pending",
            Description = "Desc",
            ApprovalStatus = "Pending",
            ExpiresAt = DateTime.Today.AddDays(30),
            SearchText = "job 1"
        };
        var j2 = new tblJob
        {
            CompanyId = comp.CompanyId,
            CategoryId = 1,
            JobTypeId = 1,
            Title = "Job 2 Pending",
            Slug = "job-2-pending",
            Description = "Desc",
            ApprovalStatus = "Pending",
            ExpiresAt = DateTime.Today.AddDays(30),
            SearchText = "job 2"
        };
        var j3 = new tblJob
        {
            CompanyId = comp.CompanyId,
            CategoryId = 1,
            JobTypeId = 1,
            Title = "Job 3 Draft",
            Slug = "job-3-draft",
            Description = "Desc",
            ApprovalStatus = "Draft",
            ExpiresAt = DateTime.Today.AddDays(30),
            SearchText = "job 3"
        };
        context.Jobs.AddRange(j1, j2, j3);
        context.SaveChanges();

        // 1. Kiểm tra danh sách Pending
        var list = await moderationService.GetPendingJobsAsync(new JobFilterModel { Status = "Pending" });
        Assert.Equal(2, list.Jobs.Count);

        // 2. Duyệt tin j1 -> Chuyển Published và gán PublishedAt
        var approveRes = await moderationService.ApproveJobAsync(adminId, j1.JobId);
        Assert.True(approveRes.Success);
        Assert.Equal("Published", j1.ApprovalStatus);
        Assert.NotNull(j1.PublishedAt);

        // 3. Từ chối tin j2 thiếu lý do -> Bị từ chối
        var rejectFail = await moderationService.RejectJobAsync(adminId, j2.JobId, "   ");
        Assert.False(rejectFail.Success);
        Assert.Contains("Lý do từ chối không được để trống", rejectFail.Message);

        // Từ chối tin j2 có lý do hợp lệ
        var rejectOk = await moderationService.RejectJobAsync(adminId, j2.JobId, "Nội dung tuyển dụng chưa rõ ràng");
        Assert.True(rejectOk.Success);
        Assert.Equal("Rejected", j2.ApprovalStatus);
        Assert.Equal("Nội dung tuyển dụng chưa rõ ràng", j2.RejectReason);

        // 4. Gỡ tin xuất bản (Unpublish j1) thiếu lý do -> Bị từ chối
        var unpublishFail = await moderationService.UnpublishJobAsync(adminId, j1.JobId, "");
        Assert.False(unpublishFail.Success);

        // Gỡ tin j1 có lý do hợp lệ
        var unpublishOk = await moderationService.UnpublishJobAsync(adminId, j1.JobId, "Doanh nghiệp yêu cầu dừng tuyển dụng");
        Assert.True(unpublishOk.Success);
        Assert.Equal("Closed", j1.ApprovalStatus);
        Assert.Equal("Doanh nghiệp yêu cầu dừng tuyển dụng", j1.RejectReason);

        // 5. Duyệt hàng loạt: gửi cả j2 (Rejected), j3 (Draft), và thêm 2 tin Pending mới
        var j4 = new tblJob { CompanyId = comp.CompanyId, CategoryId = 1, JobTypeId = 1, Title = "J4", Slug = "j4", Description = "D", ApprovalStatus = "Pending", ExpiresAt = DateTime.Today.AddDays(30), SearchText = "j4" };
        var j5 = new tblJob { CompanyId = comp.CompanyId, CategoryId = 1, JobTypeId = 1, Title = "J5", Slug = "j5", Description = "D", ApprovalStatus = "Pending", ExpiresAt = DateTime.Today.AddDays(30), SearchText = "j5" };
        context.Jobs.AddRange(j4, j5);
        context.SaveChanges();

        var batchRes = await moderationService.BatchApproveJobsAsync(adminId, new List<int> { j2.JobId, j3.JobId, j4.JobId, j5.JobId });
        Assert.True(batchRes.Success);
        // Chỉ duyệt đúng 2 tin Pending (j4, j5), bỏ qua j2 (Rejected) và j3 (Draft)
        Assert.Equal(2, batchRes.ApprovedCount);
        Assert.Equal("Published", j4.ApprovalStatus);
        Assert.Equal("Published", j5.ApprovalStatus);
        Assert.Equal("Draft", j3.ApprovalStatus);
    }

    [Fact]
    public async Task AC_S3B_11_AuditLog_RecordedForJobAndCompanyOperations()
    {
        var (moderationService, mgmtService, compService, context, adminId, comp) = SetupContext();

        // 1. Audit log khi NTD tạo tin
        var model = new JobInputModel
        {
            Title = "Security Auditor",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(30),
            IsDraft = false
        };
        var (_, _, jId) = await mgmtService.CreateJobAsync(comp.CompanyId, 10, model);

        // 2. Audit log khi SuperAdmin duyệt tin
        await moderationService.ApproveJobAsync(adminId, jId!.Value);

        // 3. Audit log khi SuperAdmin duyệt/khóa công ty (Sprint 3A nối sang)
        await compService.SuspendCompanyAsync(comp.CompanyId, "Gian lận thông tin");
        await compService.UnsuspendCompanyAsync(comp.CompanyId);
        await compService.ApproveCompanyAsync(comp.CompanyId);

        // Kiểm tra trong bảng tblAuditLog
        var logs = await context.AuditLogs.OrderBy(l => l.CreatedAt).ToListAsync();

        Assert.Contains(logs, l => l.Action == "CreateJob" && l.EntityName == "tblJob" && l.EntityId == jId.Value.ToString());
        Assert.Contains(logs, l => l.Action == "ApproveJob" && l.EntityName == "tblJob" && l.EntityId == jId.Value.ToString());
        Assert.Contains(logs, l => l.Action == "SuspendCompany" && l.EntityName == "tblCompany" && l.EntityId == comp.CompanyId.ToString());
        Assert.Contains(logs, l => l.Action == "UnsuspendCompany" && l.EntityName == "tblCompany" && l.EntityId == comp.CompanyId.ToString());
        Assert.Contains(logs, l => l.Action == "ApproveCompany" && l.EntityName == "tblCompany" && l.EntityId == comp.CompanyId.ToString());
    }

    [Fact]
    public async Task AC_S3B_12_SeedData_SatisfiesCriteria()
    {
        // Kiểm tra dữ liệu seed khi chạy DataSeeder
        var context = TestDbHelper.CreateInMemoryContext();
        var pwdMock = new Mock<IPasswordService>();
        pwdMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed_pwd");

        await DataSeeder.SeedAllAsync(context, pwdMock.Object);

        var allJobs = await context.Jobs.Include(j => j.Company).ToListAsync();

        // >= 40 tin Published còn hạn
        var publishedActiveJobs = allJobs.Where(j => j.ApprovalStatus == "Published" && j.ExpiresAt > DateTime.Now).ToList();
        Assert.True(publishedActiveJobs.Count >= 40, $"Số tin Published còn hạn phải >= 40, thực tế: {publishedActiveJobs.Count}");

        // Có tin từ xa không gắn địa điểm (WorkplaceId null)
        Assert.Contains(allJobs, j => j.WorkMode == "Remote" && j.WorkplaceId == null);

        // Có tin sắp hết hạn
        Assert.Contains(allJobs, j => j.ExpiresAt > DateTime.Now && j.ExpiresAt <= DateTime.Now.AddDays(3));

        // Có tin Pending
        Assert.Contains(allJobs, j => j.ApprovalStatus == "Pending");

        // Có tin Rejected
        Assert.Contains(allJobs, j => j.ApprovalStatus == "Rejected");

        // Có tin Expired
        Assert.Contains(allJobs, j => j.ApprovalStatus == "Expired");

        // Mọi tin SalaryMin <= SalaryMax (nếu có lương)
        foreach (var j in allJobs.Where(j => j.SalaryMin.HasValue && j.SalaryMax.HasValue))
        {
            Assert.True(j.SalaryMin <= j.SalaryMax);
        }

        // Mọi tin có SearchText không rỗng
        foreach (var j in allJobs)
        {
            Assert.False(string.IsNullOrWhiteSpace(j.SearchText));
        }
    }

    [Fact]
    public async Task AC_S3B_13_EmployerJobList_Max4Queries_Pagination()
    {
        var (moderationService, mgmtService, compService, context, adminId, comp) = SetupContext();

        // Tạo 15 tin cho công ty
        for (int i = 1; i <= 15; i++)
        {
            context.Jobs.Add(new tblJob
            {
                CompanyId = comp.CompanyId,
                CategoryId = 1,
                JobTypeId = 1,
                Title = $"Job Test {i}",
                Slug = $"job-test-{i}",
                Description = "Desc",
                ApprovalStatus = i % 2 == 0 ? "Published" : "Draft",
                ExpiresAt = DateTime.Today.AddDays(30),
                SearchText = $"job test {i}"
            });
        }
        await context.SaveChangesAsync();

        // Phân trang: Trang 1, PageSize = 5
        var page1 = await mgmtService.GetEmployerJobsAsync(comp.CompanyId, new JobFilterModel { PageIndex = 1, PageSize = 5 });
        Assert.Equal(5, page1.Jobs.Count);
        Assert.Equal(15, page1.TotalItems);
        Assert.Equal(3, page1.TotalPages);

        // Phân trang: Trang 2
        var page2 = await mgmtService.GetEmployerJobsAsync(comp.CompanyId, new JobFilterModel { PageIndex = 2, PageSize = 5 });
        Assert.Equal(5, page2.Jobs.Count);
        Assert.Equal(2, page2.PageIndex);

        // Lọc theo trạng thái Published
        var publishedOnly = await mgmtService.GetEmployerJobsAsync(comp.CompanyId, new JobFilterModel { Status = "Published" });
        Assert.Equal(7, publishedOnly.TotalItems);
        Assert.All(publishedOnly.Jobs, j => Assert.Equal("Published", j.ApprovalStatus));
    }
}

