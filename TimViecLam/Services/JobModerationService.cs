using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt nghiệp vụ kiểm duyệt tin tuyển dụng phía SuperAdmin.
/// </summary>
public class JobModerationService : IJobModerationService
{
    private readonly DataContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<JobModerationService> _logger;

    public JobModerationService(
        DataContext context,
        IAuditLogService auditLogService,
        ILogger<JobModerationService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<JobModerationListViewModel> GetPendingJobsAsync(JobFilterModel filter)
    {
        filter ??= new JobFilterModel();
        int pageIndex = filter.PageIndex > 0 ? filter.PageIndex : 1;
        int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var query = _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.JobType)
            .Include(j => j.Workplace)
            .AsNoTracking();

        int pendingCount = await _context.Jobs.CountAsync(j => j.ApprovalStatus == "Pending");

        // Nếu người dùng không chỉ định trạng thái cụ thể, mặc định hiển thị danh sách Pending
        if (string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(j => j.ApprovalStatus == "Pending");
        }
        else if (!string.Equals(filter.Status, "All", StringComparison.OrdinalIgnoreCase))
        {
            string s = filter.Status.Trim();
            query = query.Where(j => j.ApprovalStatus == s);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            string kw = filter.Keyword.Trim();
            query = query.Where(j => j.Title.Contains(kw) || (j.Company != null && j.Company.Name.Contains(kw)));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(j => j.CategoryId == filter.CategoryId.Value);
        }

        int totalItems = await query.CountAsync();

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobModerationItemDto
            {
                JobId = j.JobId,
                Title = j.Title,
                Slug = j.Slug,
                CompanyId = j.CompanyId,
                CompanyName = j.Company != null ? j.Company.Name : string.Empty,
                CompanyLogo = j.Company != null ? j.Company.Logo : null,
                CompanyStatus = j.Company != null ? j.Company.Status : string.Empty,
                CategoryName = j.Category != null ? j.Category.Name : string.Empty,
                JobTypeName = j.JobType != null ? j.JobType.Name : string.Empty,
                WorkplaceName = j.Workplace != null ? j.Workplace.Name : (j.WorkMode == "Remote" ? "Làm việc từ xa" : "Toàn quốc"),
                SalaryDisplay = j.IsSalaryNegotiable
                    ? "Thỏa thuận"
                    : (j.SalaryMin.HasValue && j.SalaryMax.HasValue
                        ? $"{j.SalaryMin.Value:N0} - {j.SalaryMax.Value:N0} {j.SalaryUnit}"
                        : (j.SalaryMin.HasValue
                            ? $"Từ {j.SalaryMin.Value:N0} {j.SalaryUnit}"
                            : (j.SalaryMax.HasValue ? $"Đến {j.SalaryMax.Value:N0} {j.SalaryUnit}" : "Thỏa thuận"))),
                WorkMode = j.WorkMode,
                ApprovalStatus = j.ApprovalStatus,
                CreatedAt = j.CreatedAt,
                ExpiresAt = j.ExpiresAt,
                RejectReason = j.RejectReason
            })
            .ToListAsync();

        return new JobModerationListViewModel
        {
            Jobs = jobs,
            Filter = filter,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize,
            PendingCount = pendingCount
        };
    }

    public async Task<JobModerationDetailsViewModel?> GetJobDetailsForModerationAsync(int jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.JobType)
            .Include(j => j.Workplace)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.JobBenefits).ThenInclude(jb => jb.Benefit)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null) return null;

        var auditLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(al => al.EntityName == "tblJob" && al.EntityId == jobId.ToString())
            .OrderByDescending(al => al.CreatedAt)
            .Take(20)
            .ToListAsync();

        return new JobModerationDetailsViewModel
        {
            JobId = job.JobId,
            Title = job.Title,
            Slug = job.Slug,
            CategoryName = job.Category != null ? job.Category.Name : string.Empty,
            JobTypeName = job.JobType != null ? job.JobType.Name : string.Empty,
            WorkplaceName = job.Workplace != null ? job.Workplace.Name : string.Empty,
            WorkplaceAddress = job.Workplace != null ? job.Workplace.Address : string.Empty,
            WorkMode = job.WorkMode,
            SalaryDisplay = job.IsSalaryNegotiable
                ? "Thỏa thuận"
                : (job.SalaryMin.HasValue && job.SalaryMax.HasValue
                    ? $"{job.SalaryMin.Value:N0} - {job.SalaryMax.Value:N0} {job.SalaryUnit}"
                    : (job.SalaryMin.HasValue
                        ? $"Từ {job.SalaryMin.Value:N0} {job.SalaryUnit}"
                        : (job.SalaryMax.HasValue ? $"Đến {job.SalaryMax.Value:N0} {job.SalaryUnit}" : "Thỏa thuận"))),
            ExperienceLevel = job.ExperienceLevel,
            EducationLevel = job.EducationLevel,
            Headcount = job.Headcount,
            ExpiresAt = job.ExpiresAt,
            ApprovalStatus = job.ApprovalStatus,
            Description = job.Description,
            Requirements = job.Requirements,
            CreatedAt = job.CreatedAt,
            PublishedAt = job.PublishedAt,
            RejectReason = job.RejectReason,
            Skills = job.JobSkills.Where(s => s.Skill != null).Select(s => s.Skill!.Name).ToList(),
            Benefits = job.JobBenefits.Where(b => b.Benefit != null).Select(b => b.Benefit!.Name).ToList(),
            CompanyId = job.CompanyId,
            CompanyName = job.Company != null ? job.Company.Name : string.Empty,
            CompanyLogo = job.Company != null ? job.Company.Logo : null,
            CompanyStatus = job.Company != null ? job.Company.Status : string.Empty,
            CompanyWebsite = job.Company != null ? job.Company.Website : null,
            CompanyAddress = job.Company != null ? job.Company.Address : null,
            AuditLogs = auditLogs
        };
    }

    public async Task<(bool Success, string? Message)> ApproveJobAsync(int adminUserId, int jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == jobId);

        if (job == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng.");
        }

        // AC-S3B.5: Kiểm tra vòng đời: chỉ tin Pending mới được duyệt sang Published
        if (job.ApprovalStatus != "Pending")
        {
            return (false, $"Chuyển trạng thái không hợp lệ: Không thể duyệt tin đang ở trạng thái '{job.ApprovalStatus}'.");
        }

        // Kiểm tra công ty phải Approved
        if (job.Company == null || job.Company.Status != "Approved")
        {
            return (false, "Công ty của tin này chưa được duyệt hoặc đang bị khóa, không thể xuất bản tin.");
        }

        job.ApprovalStatus = "Published";
        job.PublishedAt = DateTime.Now;
        job.RejectReason = null;
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // Ghi AuditLog (AC-S3B.11)
        await _auditLogService.LogAsync(adminUserId, "ApproveJob", "tblJob", jobId.ToString(), "Phê duyệt và xuất bản tin tuyển dụng");
        _logger.LogInformation("SuperAdmin {AdminId} đã duyệt tin tuyển dụng {JobId} ({Title})", adminUserId, jobId, job.Title);

        return (true, "Phê duyệt và xuất bản tin tuyển dụng thành công.");
    }

    public async Task<(bool Success, string? Message)> RejectJobAsync(int adminUserId, int jobId, string reason)
    {
        // AC-S3B.10: Từ chối bắt buộc có lý do
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Lý do từ chối không được để trống.");
        }

        string cleanReason = reason.Trim();
        if (cleanReason.Length < 5)
        {
            return (false, "Lý do từ chối phải có ít nhất 5 ký tự.");
        }

        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng.");
        }

        if (job.ApprovalStatus != "Pending")
        {
            return (false, $"Chuyển trạng thái không hợp lệ: Chỉ có thể từ chối tin đang ở trạng thái 'Pending'. Hiện tại là '{job.ApprovalStatus}'.");
        }

        job.ApprovalStatus = "Rejected";
        job.RejectReason = cleanReason;
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // Ghi AuditLog
        await _auditLogService.LogAsync(adminUserId, "RejectJob", "tblJob", jobId.ToString(), $"Từ chối duyệt tin: {cleanReason}");
        _logger.LogInformation("SuperAdmin {AdminId} đã từ chối tin tuyển dụng {JobId} với lý do: {Reason}", adminUserId, jobId, cleanReason);

        return (true, "Đã từ chối tin tuyển dụng.");
    }

    public async Task<(bool Success, string? Message)> UnpublishJobAsync(int adminUserId, int jobId, string reason)
    {
        // Gỡ tin bắt buộc có lý do
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Lý do gỡ tin không được để trống.");
        }

        string cleanReason = reason.Trim();
        if (cleanReason.Length < 5)
        {
            return (false, "Lý do gỡ tin phải có ít nhất 5 ký tự.");
        }

        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng.");
        }

        if (job.ApprovalStatus != "Published")
        {
            return (false, "Chỉ có thể gỡ tin đang ở trạng thái xuất bản (Published).");
        }

        job.ApprovalStatus = "Closed";
        job.RejectReason = cleanReason;
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // Ghi AuditLog
        await _auditLogService.LogAsync(adminUserId, "UnpublishJob", "tblJob", jobId.ToString(), $"Gỡ tin tuyển dụng: {cleanReason}");
        _logger.LogInformation("SuperAdmin {AdminId} đã gỡ tin tuyển dụng {JobId} với lý do: {Reason}", adminUserId, jobId, cleanReason);

        return (true, "Đã gỡ tin tuyển dụng thành công.");
    }

    public async Task<(bool Success, string? Message, int ApprovedCount)> BatchApproveJobsAsync(int adminUserId, List<int> jobIds)
    {
        if (jobIds == null || jobIds.Count == 0)
        {
            return (false, "Vui lòng chọn ít nhất một tin tuyển dụng để duyệt.", 0);
        }

        // AC-S3B.10: Chỉ xử lý tin có trạng thái Pending, bỏ qua các tin khác
        var pendingJobs = await _context.Jobs
            .Include(j => j.Company)
            .Where(j => jobIds.Contains(j.JobId) && j.ApprovalStatus == "Pending")
            .ToListAsync();

        if (pendingJobs.Count == 0)
        {
            return (false, "Không có tin tuyển dụng nào ở trạng thái chờ duyệt (Pending) trong danh sách đã chọn.", 0);
        }

        int approvedCount = 0;
        var now = DateTime.Now;

        foreach (var job in pendingJobs)
        {
            // Bỏ qua nếu công ty chưa Approved
            if (job.Company == null || job.Company.Status != "Approved")
            {
                continue;
            }

            job.ApprovalStatus = "Published";
            job.PublishedAt = now;
            job.RejectReason = null;
            job.UpdatedAt = now;

            approvedCount++;

            // Ghi AuditLog cho từng tin được duyệt
            await _auditLogService.LogAsync(adminUserId, "ApproveJob", "tblJob", job.JobId.ToString(), "Phê duyệt hàng loạt bởi SuperAdmin");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("SuperAdmin {AdminId} đã duyệt hàng loạt {Count} tin tuyển dụng", adminUserId, approvedCount);

        return (true, $"Đã phê duyệt và xuất bản thành công {approvedCount} tin tuyển dụng.", approvedCount);
    }
}

