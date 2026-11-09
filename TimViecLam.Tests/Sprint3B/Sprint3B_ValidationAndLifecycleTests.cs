using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Extensions;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3B;

public class Sprint3B_ValidationAndLifecycleTests
{
    private (JobManagementService service, DataContext context, tblCompany approvedComp, tblCompany pendingComp) SetupContext(bool autoApprove = false)
    {
        var context = TestDbHelper.CreateInMemoryContext();

        var appComp = new tblCompany
        {
            Name = "Approved Tech",
            Slug = "approved-tech",
            Status = "Approved",
            CreatedAt = DateTime.Now
        };
        var pendComp = new tblCompany
        {
            Name = "Pending Corp",
            Slug = "pending-corp",
            Status = "Pending",
            CreatedAt = DateTime.Now
        };
        var suspComp = new tblCompany
        {
            Name = "Suspended Corp",
            Slug = "suspended-corp",
            Status = "Suspended",
            CreatedAt = DateTime.Now
        };
        context.Companies.AddRange(appComp, pendComp, suspComp);

        var cat = new tblJobCategory { Name = "Công nghệ thông tin", Slug = "cntt", IsActive = true };
        var type = new tblJobType { Name = "Toàn thời gian", Code = "FullTime", Slug = "toan-thoi-gian", IsActive = true };
        context.JobCategories.Add(cat);
        context.JobTypes.Add(type);

        for (int i = 1; i <= 20; i++)
        {
            context.Skills.Add(new tblSkill { Name = $"Skill {i}", Slug = $"skill-{i}", IsActive = true });
        }
        context.SaveChanges();

        var sanitizer = new HtmlSanitizerService();
        var auditMock = new Mock<IAuditLogService>();
        var config = new ConfigurationBuilder().Build();
        var options = Options.Create(new ListingSettings { AutoApprove = autoApprove });

        var service = new JobManagementService(
            context,
            sanitizer,
            auditMock.Object,
            options,
            config,
            NullLogger<JobManagementService>.Instance);

        return (service, context, appComp, pendComp);
    }

    [Fact]
    public async Task AC_S3B_3_ValidateServer_SalaryMinMax_ExpiresAt_SkillsMax15_Title()
    {
        var (service, context, comp, _) = SetupContext();

        // 1. Min salary > Max salary
        var m1 = new JobInputModel
        {
            Title = "Dev 1",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            SalaryMin = 20000000,
            SalaryMax = 10000000,
            IsSalaryNegotiable = false,
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var r1 = await service.CreateJobAsync(comp.CompanyId, 1, m1);
        Assert.False(r1.Success);
        Assert.Contains("tối thiểu không được lớn hơn", r1.Message);

        // 2. ExpiresAt trong quá khứ
        var m2 = new JobInputModel
        {
            Title = "Dev 2",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(-1)
        };
        var r2 = await service.CreateJobAsync(comp.CompanyId, 1, m2);
        Assert.False(r2.Success);
        Assert.Contains("quá khứ", r2.Message);

        // 3. ExpiresAt vượt quá 90 ngày
        var m3 = new JobInputModel
        {
            Title = "Dev 3",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(95)
        };
        var r3 = await service.CreateJobAsync(comp.CompanyId, 1, m3);
        Assert.False(r3.Success);
        Assert.Contains("tối đa là 90 ngày", r3.Message);

        // 4. Quá 15 kỹ năng (> 15)
        var m4 = new JobInputModel
        {
            Title = "Dev 4",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(30),
            SkillIds = Enumerable.Range(1, 16).ToList()
        };
        var r4 = await service.CreateJobAsync(comp.CompanyId, 1, m4);
        Assert.False(r4.Success);
        Assert.Contains("tối đa 15 kỹ năng", r4.Message);

        // 5. Tiêu đề rỗng hoặc quá dài
        var m5 = new JobInputModel
        {
            Title = "   ",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var r5 = await service.CreateJobAsync(comp.CompanyId, 1, m5);
        Assert.False(r5.Success);
        Assert.Contains("Tiêu đề", r5.Message);
    }

    [Fact]
    public void AC_S3B_4_HtmlSanitizer_Strips_Script_EventHandlers_PreservesSafeTags()
    {
        var sanitizer = new HtmlSanitizerService();

        // Chuỗi tấn công XSS chứa <script>, onerror, javascript:
        string maliciousHtml = "<p>Mô tả công việc: <b>Lập trình C#</b></p>" +
                               "<script>alert('xss');</script>" +
                               "<img src='invalid.jpg' onerror='alert(document.cookie);' />" +
                               "<a href='javascript:stealTokens()'>Bấm vào đây</a>" +
                               "<ul><li>Lương thưởng hấp dẫn</li></ul>";

        string cleanHtml = sanitizer.Sanitize(maliciousHtml);

        // Phải loại bỏ mã độc
        Assert.DoesNotContain("<script>", cleanHtml);
        Assert.DoesNotContain("alert('xss')", cleanHtml);
        Assert.DoesNotContain("onerror", cleanHtml);
        Assert.DoesNotContain("javascript:", cleanHtml);

        // Phải giữ nguyên thẻ HTML an toàn
        Assert.Contains("<p>Mô tả công việc: <b>Lập trình C#</b></p>", cleanHtml);
        Assert.Contains("<ul><li>Lương thưởng hấp dẫn</li></ul>", cleanHtml);
    }

    [Fact]
    public async Task AC_S3B_5_Lifecycle_ValidTransitions_CriticalFieldsRevertsToPending_AutoApprove()
    {
        // 1. Kiểm tra AutoApprove = false (Mặc định gửi duyệt -> Pending)
        var (service, context, comp, _) = SetupContext(autoApprove: false);

        var model = new JobInputModel
        {
            Title = "Frontend Engineer React",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "<p>Mô tả chi tiết</p>",
            SalaryMin = 15000000,
            SalaryMax = 25000000,
            ExpiresAt = DateTime.Today.AddDays(30),
            IsDraft = false
        };

        var (s1, _, jId) = await service.CreateJobAsync(comp.CompanyId, 1, model);
        Assert.True(s1);
        var job = context.Jobs.Find(jId!.Value);
        Assert.Equal("Pending", job!.ApprovalStatus);

        // Giả lập admin duyệt -> Published
        job.ApprovalStatus = "Published";
        job.PublishedAt = DateTime.Now;
        context.SaveChanges();

        // Sửa thông tin quan trọng (Tiêu đề / Lương) -> Phải tự động quay về Pending
        var updateModel = new JobInputModel
        {
            Title = "Senior Frontend Engineer React (Updated)",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "<p>Mô tả chi tiết</p>",
            SalaryMin = 20000000, // Đổi lương
            SalaryMax = 35000000,
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var updateRes = await service.UpdateJobAsync(comp.CompanyId, 1, job.JobId, updateModel);
        Assert.True(updateRes.Success);
        Assert.Equal("Pending", job.ApprovalStatus);

        // 2. Chuyển trạng thái sai (VD: Thử gửi duyệt tin đang Pending -> Phải từ chối)
        var invalidSend = await service.SendToApprovalAsync(comp.CompanyId, 1, job.JobId);
        Assert.False(invalidSend.Success);
        Assert.Contains("không hợp lệ", invalidSend.Message);

        // 3. Kiểm tra AutoApprove = true -> Tự động xuất bản Published
        var (serviceAuto, contextAuto, compAuto, _) = SetupContext(autoApprove: true);
        var (s2, _, jIdAuto) = await serviceAuto.CreateJobAsync(compAuto.CompanyId, 1, model);
        Assert.True(s2);
        var jobAuto = contextAuto.Jobs.Find(jIdAuto!.Value);
        Assert.Equal("Published", jobAuto!.ApprovalStatus);
        Assert.NotNull(jobAuto.PublishedAt);
    }

    [Fact]
    public async Task AC_S3B_6_NTD_Pending_Suspended_Rejected_CannotCreateJobs()
    {
        var (service, context, _, pendingComp) = SetupContext();

        var model = new JobInputModel
        {
            Title = "Dev Rejected",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(30)
        };

        // Công ty Pending tạo tin -> Từ chối
        var r1 = await service.CreateJobAsync(pendingComp.CompanyId, 2, model);
        Assert.False(r1.Success);
        Assert.Contains("chưa được duyệt hoặc đang bị tạm khóa", r1.Message);

        // Đổi sang Suspended -> Từ chối
        pendingComp.Status = "Suspended";
        context.SaveChanges();
        var r2 = await service.CreateJobAsync(pendingComp.CompanyId, 2, model);
        Assert.False(r2.Success);

        // Đổi sang Rejected -> Từ chối
        pendingComp.Status = "Rejected";
        context.SaveChanges();
        var r3 = await service.CreateJobAsync(pendingComp.CompanyId, 2, model);
        Assert.False(r3.Success);
    }

    [Fact]
    public async Task AC_S3B_7_UniqueSlug_SearchText_UpdatedOnTitleChange()
    {
        var (service, context, comp, _) = SetupContext();

        var m1 = new JobInputModel
        {
            Title = "Dev .NET Senior",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc 1",
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var (_, _, j1) = await service.CreateJobAsync(comp.CompanyId, 1, m1);
        var job1 = context.Jobs.Find(j1!.Value);
        Assert.Equal("dev-net-senior", job1!.Slug);
        Assert.Contains("dev .net senior", job1.SearchText);

        // Tạo tin thứ 2 trùng tiêu đề -> Tự động thêm hậu tố -1
        var m2 = new JobInputModel
        {
            Title = "Dev .NET Senior",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc 2",
            ExpiresAt = DateTime.Today.AddDays(30)
        };
        var (_, _, j2) = await service.CreateJobAsync(comp.CompanyId, 1, m2);
        var job2 = context.Jobs.Find(j2!.Value);
        Assert.Equal("dev-net-senior-1", job2!.Slug);

        // Sửa tiêu đề job 1 -> Slug và SearchText tự động cập nhật
        m1.Title = "Lead Architect .NET";
        await service.UpdateJobAsync(comp.CompanyId, 1, job1.JobId, m1);
        Assert.Equal("lead-architect-net", job1.Slug);
        Assert.Contains("lead architect .net", job1.SearchText);
    }

    [Fact]
    public void AC_S3B_8_WhereVisibleToPublic_FiltersDraftPendingExpiredSuspended()
    {
        var context = TestDbHelper.CreateInMemoryContext();

        var compApp = new tblCompany { Name = "Comp OK", Slug = "comp-ok", Status = "Approved" };
        var compSusp = new tblCompany { Name = "Comp Susp", Slug = "comp-susp", Status = "Suspended" };
        context.Companies.AddRange(compApp, compSusp);

        var cat = new tblJobCategory { Name = "IT", Slug = "it" };
        var type = new tblJobType { Name = "Full", Code = "FT", Slug = "ft" };
        context.JobCategories.Add(cat);
        context.JobTypes.Add(type);
        context.SaveChanges();

        // 1. Tin Published hợp lệ
        var validJob = new tblJob
        {
            Company = compApp,
            Category = cat,
            JobType = type,
            Title = "Valid Job",
            Slug = "valid-job",
            Description = "Desc",
            ApprovalStatus = "Published",
            ExpiresAt = DateTime.Today.AddDays(20),
            SearchText = "valid"
        };
        // 2. Tin Draft
        var draftJob = new tblJob
        {
            Company = compApp,
            Category = cat,
            JobType = type,
            Title = "Draft Job",
            Slug = "draft-job",
            Description = "Desc",
            ApprovalStatus = "Draft",
            ExpiresAt = DateTime.Today.AddDays(20),
            SearchText = "draft"
        };
        // 3. Tin Pending
        var pendingJob = new tblJob
        {
            Company = compApp,
            Category = cat,
            JobType = type,
            Title = "Pending Job",
            Slug = "pending-job",
            Description = "Desc",
            ApprovalStatus = "Pending",
            ExpiresAt = DateTime.Today.AddDays(20),
            SearchText = "pending"
        };
        // 4. Tin quá hạn (Expired date)
        var expiredJob = new tblJob
        {
            Company = compApp,
            Category = cat,
            JobType = type,
            Title = "Expired Date Job",
            Slug = "expired-job",
            Description = "Desc",
            ApprovalStatus = "Published",
            ExpiresAt = DateTime.Today.AddDays(-2),
            SearchText = "expired"
        };
        // 5. Tin của công ty bị Suspended
        var suspCompJob = new tblJob
        {
            Company = compSusp,
            Category = cat,
            JobType = type,
            Title = "Susp Comp Job",
            Slug = "susp-comp-job",
            Description = "Desc",
            ApprovalStatus = "Published",
            ExpiresAt = DateTime.Today.AddDays(20),
            SearchText = "susp"
        };

        context.Jobs.AddRange(validJob, draftJob, pendingJob, expiredJob, suspCompJob);
        context.SaveChanges();

        // Áp dụng WhereVisibleToPublic()
        var visibleJobs = context.Jobs.Include(j => j.Company).WhereVisibleToPublic().ToList();

        // Chỉ duy nhất validJob được xuất hiện
        Assert.Single(visibleJobs);
        Assert.Equal("valid-job", visibleJobs[0].Slug);
    }

    [Fact]
    public async Task AC_S3B_9_DeleteBlockedIfApplicationsExist_Reopen_DuplicateDraft()
    {
        var (service, context, comp, _) = SetupContext();

        var model = new JobInputModel
        {
            Title = "Job With Apps",
            CategoryId = 1,
            JobTypeId = 1,
            Description = "Desc",
            ExpiresAt = DateTime.Today.AddDays(30),
            IsDraft = false
        };
        var (_, _, jId) = await service.CreateJobAsync(comp.CompanyId, 1, model);
        int jobId = jId!.Value;

        // Chèn trực tiếp một đơn ứng tuyển vào DB cho tin này
        var app = new tblApplication
        {
            JobId = jobId,
            CandidateUserId = 999,
            CoverLetter = "Em xin ứng tuyển",
            Status = "Submitted",
            AppliedAt = DateTime.Now
        };
        context.Applications.Add(app);
        context.SaveChanges();

        // 1. Thử xóa tin đã có đơn -> Bị chặn
        var deleteRes = await service.DeleteJobAsync(comp.CompanyId, 1, jobId);
        Assert.False(deleteRes.Success);
        Assert.Contains("Không thể xóa tin tuyển dụng đã có ứng viên nộp hồ sơ", deleteRes.Message);

        // 2. Thử đóng tin và mở lại
        var job = context.Jobs.Find(jobId);
        job!.ApprovalStatus = "Published";
        context.SaveChanges();

        var closeRes = await service.CloseJobAsync(comp.CompanyId, 1, jobId);
        Assert.True(closeRes.Success);
        Assert.Equal("Closed", job.ApprovalStatus);

        var reopenRes = await service.ReopenJobAsync(comp.CompanyId, 1, jobId);
        Assert.True(reopenRes.Success);
        Assert.Equal("Pending", job.ApprovalStatus);

        // 3. Nhân bản tin -> Sinh bản nháp mới
        var dupRes = await service.DuplicateJobAsync(comp.CompanyId, 1, jobId);
        Assert.True(dupRes.Success);
        Assert.NotNull(dupRes.NewJobId);

        var dupJob = context.Jobs.Find(dupRes.NewJobId.Value);
        Assert.Equal("Draft", dupJob!.ApprovalStatus);
        Assert.Contains("Bản sao", dupJob.Title);
        Assert.NotEqual(job.Slug, dupJob.Slug);
    }
}
