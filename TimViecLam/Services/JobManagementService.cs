using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt nghiệp vụ quản lý tin tuyển dụng phía Nhà tuyển dụng.
/// </summary>
public class JobManagementService : IJobManagementService
{
    private readonly DataContext _context;
    private readonly IHtmlSanitizerService _sanitizer;
    private readonly IAuditLogService _auditLogService;
    private readonly ListingSettings _listingSettings;
    private readonly ILogger<JobManagementService> _logger;

    public JobManagementService(
        DataContext context,
        IHtmlSanitizerService sanitizer,
        IAuditLogService auditLogService,
        IOptions<ListingSettings> listingOptions,
        IConfiguration configuration,
        ILogger<JobManagementService> logger)
    {
        _context = context;
        _sanitizer = sanitizer;
        _auditLogService = auditLogService;
        _logger = logger;

        // Ưu tiên đọc cấu hình từ Options, fallback sang Configuration
        _listingSettings = listingOptions.Value ?? new ListingSettings();
        if (configuration.GetSection("ListingSettings:AutoApprove").Exists())
        {
            _listingSettings.AutoApprove = configuration.GetValue<bool>("ListingSettings:AutoApprove");
        }
    }

    public async Task<EmployerJobListViewModel> GetEmployerJobsAsync(int companyId, JobFilterModel filter)
    {
        filter ??= new JobFilterModel();
        int pageIndex = filter.PageIndex > 0 ? filter.PageIndex : 1;
        int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        // Truy vấn gốc giới hạn nghiêm ngặt theo CompanyId của NTD (Chống IDOR)
        var baseQuery = _context.Jobs
            .AsNoTracking()
            .Where(j => j.CompanyId == companyId);

        // Đếm số lượng theo từng nhóm trạng thái để hiển thị badge tab
        var statusCountsRaw = await baseQuery
            .GroupBy(j => j.ApprovalStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var statusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "All", statusCountsRaw.Sum(x => x.Count) },
            { "Published", statusCountsRaw.FirstOrDefault(x => x.Status == "Published")?.Count ?? 0 },
            { "Pending", statusCountsRaw.FirstOrDefault(x => x.Status == "Pending")?.Count ?? 0 },
            { "Draft", statusCountsRaw.FirstOrDefault(x => x.Status == "Draft")?.Count ?? 0 },
            { "Rejected", statusCountsRaw.FirstOrDefault(x => x.Status == "Rejected")?.Count ?? 0 },
            { "Closed", statusCountsRaw.FirstOrDefault(x => x.Status == "Closed")?.Count ?? 0 },
            { "Expired", statusCountsRaw.FirstOrDefault(x => x.Status == "Expired")?.Count ?? 0 }
        };

        // Áp dụng bộ lọc
        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(filter.Status) && !string.Equals(filter.Status, "All", StringComparison.OrdinalIgnoreCase))
        {
            string s = filter.Status.Trim();
            query = query.Where(j => j.ApprovalStatus == s);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            string kw = filter.Keyword.Trim();
            query = query.Where(j => j.Title.Contains(kw));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(j => j.CategoryId == filter.CategoryId.Value);
        }

        if (filter.JobTypeId.HasValue && filter.JobTypeId.Value > 0)
        {
            query = query.Where(j => j.JobTypeId == filter.JobTypeId.Value);
        }

        // Truy vấn 1: Đếm tổng số bản ghi
        int totalItems = await query.CountAsync();

        // Truy vấn 2: Lấy trang dữ liệu hiện tại
        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new EmployerJobItemDto
            {
                JobId = j.JobId,
                Title = j.Title,
                Slug = j.Slug,
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
                ExpiresAt = j.ExpiresAt,
                ApprovalStatus = j.ApprovalStatus,
                ViewCount = j.ViewCount,
                ApplicationCount = j.Applications.Count,
                CreatedAt = j.CreatedAt,
                PublishedAt = j.PublishedAt,
                IsFeatured = j.IsFeatured,
                RejectReason = j.RejectReason
            })
            .ToListAsync();

        return new EmployerJobListViewModel
        {
            Jobs = jobs,
            Filter = filter,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize,
            StatusCounts = statusCounts
        };
    }

    public async Task<JobInputModel?> GetJobInputForCreateAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null) return null;

        return new JobInputModel
        {
            ExpiresAt = DateTime.Today.AddDays(30),
            WorkMode = "OnSite",
            SalaryUnit = "VND",
            ExperienceLevel = "Không yêu cầu",
            EducationLevel = "Không yêu cầu",
            Headcount = 1
        };
    }

    public async Task<(bool Success, string? Message, int? JobId)> CreateJobAsync(int companyId, int userId, JobInputModel model)
    {
        // 1. Kiểm tra công ty có hợp lệ và đã được duyệt (AC-S3B.6)
        var company = await _context.Companies
            .Include(c => c.Workplaces)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId);

        if (company == null)
        {
            return (false, "Không tìm thấy thông tin công ty.", null);
        }

        if (company.Status != "Approved")
        {
            return (false, "Công ty của bạn chưa được duyệt hoặc đang bị tạm khóa, không thể tạo hoặc gửi tin tuyển dụng.", null);
        }

        // 2. Validate nghiệp vụ server (AC-S3B.3)
        var validateMsg = ValidateJobInput(model);
        if (!string.IsNullOrEmpty(validateMsg))
        {
            return (false, validateMsg, null);
        }

        // Nếu có chọn chi nhánh, kiểm tra chi nhánh đó có thuộc công ty này không (chống IDOR)
        if (model.WorkplaceId.HasValue && model.WorkplaceId.Value > 0)
        {
            bool wpValid = company.Workplaces.Any(w => w.WorkplaceId == model.WorkplaceId.Value);
            if (!wpValid)
            {
                return (false, "Địa điểm làm việc được chọn không thuộc công ty của bạn.", null);
            }
        }

        // 3. Làm sạch mã HTML (AC-S3B.4)
        string cleanDesc = _sanitizer.Sanitize(model.Description);
        string? cleanReq = _sanitizer.Sanitize(model.Requirements);
        string cleanTitle = model.Title.Trim();

        // 4. Sinh Slug duy nhất
        string uniqueSlug = await GenerateUniqueSlugAsync(cleanTitle);

        // 5. Tính toán chuỗi tìm kiếm không dấu SearchText
        var skillNames = await GetSkillNamesAsync(model.SkillIds);
        var wp = model.WorkplaceId.HasValue ? company.Workplaces.FirstOrDefault(w => w.WorkplaceId == model.WorkplaceId.Value) : null;
        string wpText = wp != null ? $"{wp.Name} {wp.Address}" : (model.WorkMode == "Remote" ? "Từ xa Remote" : string.Empty);
        string searchText = SearchTextBuilder.BuildFromParts(cleanTitle, company.Name, skillNames, wpText);

        // 6. Xác định trạng thái ban đầu dựa trên IsDraft và ListingSettings:AutoApprove (AC-S3B.5)
        string status;
        DateTime? publishedAt = null;

        if (model.IsDraft)
        {
            status = "Draft";
        }
        else
        {
            if (_listingSettings.AutoApprove)
            {
                status = "Published";
                publishedAt = DateTime.Now;
            }
            else
            {
                status = "Pending";
            }
        }

        // 7. Tạo thực thể (Gán giá trị bảo mật từ server, chống Over-posting)
        var job = new tblJob
        {
            CompanyId = companyId,
            WorkplaceId = (model.WorkMode == "Remote" && (!model.WorkplaceId.HasValue || model.WorkplaceId.Value <= 0)) ? null : model.WorkplaceId,
            CategoryId = model.CategoryId,
            JobTypeId = model.JobTypeId,
            Title = cleanTitle,
            Slug = uniqueSlug,
            Description = cleanDesc,
            Requirements = cleanReq,
            SalaryMin = model.IsSalaryNegotiable ? null : model.SalaryMin,
            SalaryMax = model.IsSalaryNegotiable ? null : model.SalaryMax,
            SalaryUnit = string.IsNullOrWhiteSpace(model.SalaryUnit) ? "VND" : model.SalaryUnit.Trim(),
            IsSalaryNegotiable = model.IsSalaryNegotiable,
            ExperienceLevel = model.ExperienceLevel,
            EducationLevel = model.EducationLevel,
            WorkMode = model.WorkMode,
            Headcount = model.Headcount > 0 ? model.Headcount : 1,
            ExpiresAt = model.ExpiresAt.Date.AddDays(1).AddSeconds(-1), // Hết hạn vào cuối ngày được chọn
            ApprovalStatus = status,
            IsFeatured = false, // Không cho client tự phong tin nổi bật
            ViewCount = 0,
            PublishedAt = publishedAt,
            SearchText = searchText,
            CreatedAt = DateTime.Now
        };

        // Gán kỹ năng
        if (model.SkillIds != null && model.SkillIds.Count > 0)
        {
            foreach (var skId in model.SkillIds.Distinct())
            {
                job.JobSkills.Add(new tblJobSkill { SkillId = skId });
            }
        }

        // Gán phúc lợi
        if (model.BenefitIds != null && model.BenefitIds.Count > 0)
        {
            foreach (var bId in model.BenefitIds.Distinct())
            {
                job.JobBenefits.Add(new tblJobBenefit { BenefitId = bId });
            }
        }

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // 8. Ghi nhật ký kiểm toán (AC-S3B.11)
        string actionName = model.IsDraft ? "CreateDraftJob" : "CreateJob";
        string logDetail = model.IsDraft ? "Tạo tin nháp" : (_listingSettings.AutoApprove ? "Đăng tin (Tự động duyệt)" : "Tạo và gửi duyệt tin");
        await _auditLogService.LogAsync(userId, actionName, "tblJob", job.JobId.ToString(), logDetail);

        string successMsg = model.IsDraft
            ? "Lưu bản nháp tin tuyển dụng thành công."
            : (_listingSettings.AutoApprove ? "Đăng tin tuyển dụng thành công (Đã xuất bản)." : "Gửi tin tuyển dụng đi phê duyệt thành công.");

        return (true, successMsg, job.JobId);
    }

    public async Task<JobInputModel?> GetJobInputForEditAsync(int companyId, int jobId)
    {
        // Kiểm tra sở hữu (Chống IDOR: công ty B xem tin của công ty A trả về null -> 404)
        var job = await _context.Jobs
            .Include(j => j.JobSkills)
            .Include(j => j.JobBenefits)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);

        if (job == null) return null;

        return new JobInputModel
        {
            JobId = job.JobId,
            Title = job.Title,
            CategoryId = job.CategoryId,
            JobTypeId = job.JobTypeId,
            WorkplaceId = job.WorkplaceId,
            WorkMode = job.WorkMode,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            SalaryUnit = job.SalaryUnit,
            IsSalaryNegotiable = job.IsSalaryNegotiable,
            ExperienceLevel = job.ExperienceLevel,
            EducationLevel = job.EducationLevel,
            Headcount = job.Headcount,
            ExpiresAt = job.ExpiresAt,
            Description = job.Description,
            Requirements = job.Requirements,
            SkillIds = job.JobSkills.Select(js => js.SkillId).ToList(),
            BenefitIds = job.JobBenefits.Select(jb => jb.BenefitId).ToList(),
            IsDraft = job.ApprovalStatus == "Draft"
        };
    }

    public async Task<(bool Success, string? Message)> UpdateJobAsync(int companyId, int userId, int jobId, JobInputModel model)
    {
        // 1. Kiểm tra tồn tại và quyền sở hữu (Chống IDOR)
        var job = await _context.Jobs
            .Include(j => j.JobSkills)
            .Include(j => j.JobBenefits)
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);

        if (job == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng.");
        }

        // 2. Validate dữ liệu nhập
        var validateMsg = ValidateJobInput(model);
        if (!string.IsNullOrEmpty(validateMsg))
        {
            return (false, validateMsg);
        }

        // Kiểm tra chi nhánh có thuộc công ty
        if (model.WorkplaceId.HasValue && model.WorkplaceId.Value > 0)
        {
            bool wpValid = await _context.Workplaces.AnyAsync(w => w.WorkplaceId == model.WorkplaceId.Value && w.CompanyId == companyId);
            if (!wpValid)
            {
                return (false, "Địa điểm làm việc được chọn không thuộc công ty của bạn.");
            }
        }

        string cleanTitle = model.Title.Trim();
        string cleanDesc = _sanitizer.Sanitize(model.Description);
        string? cleanReq = _sanitizer.Sanitize(model.Requirements);

        // 3. Kiểm tra các trường quan trọng có bị thay đổi không (AC-S3B.5)
        // Các trường quan trọng: Tiêu đề, Mô tả, Mức lương, Địa điểm, Chế độ làm việc
        bool criticalChanged = !string.Equals(job.Title, cleanTitle, StringComparison.Ordinal)
            || !string.Equals(job.Description, cleanDesc, StringComparison.Ordinal)
            || job.SalaryMin != (model.IsSalaryNegotiable ? null : model.SalaryMin)
            || job.SalaryMax != (model.IsSalaryNegotiable ? null : model.SalaryMax)
            || job.WorkplaceId != model.WorkplaceId
            || job.WorkMode != model.WorkMode;

        // Nếu tin đang Published mà sửa trường quan trọng:
        // Chuyển về Pending (trừ khi AutoApprove == true)
        if (job.ApprovalStatus == "Published" && criticalChanged)
        {
            if (_listingSettings.AutoApprove)
            {
                job.ApprovalStatus = "Published";
                job.PublishedAt = DateTime.Now;
            }
            else
            {
                job.ApprovalStatus = "Pending";
            }
        }

        // Cập nhật Slug nếu đổi tiêu đề
        if (!string.Equals(job.Title, cleanTitle, StringComparison.OrdinalIgnoreCase))
        {
            job.Slug = await GenerateUniqueSlugAsync(cleanTitle, jobId);
        }

        // Cập nhật các trường thông tin
        job.Title = cleanTitle;
        job.CategoryId = model.CategoryId;
        job.JobTypeId = model.JobTypeId;
        job.WorkplaceId = (model.WorkMode == "Remote" && (!model.WorkplaceId.HasValue || model.WorkplaceId.Value <= 0)) ? null : model.WorkplaceId;
        job.WorkMode = model.WorkMode;
        job.SalaryMin = model.IsSalaryNegotiable ? null : model.SalaryMin;
        job.SalaryMax = model.IsSalaryNegotiable ? null : model.SalaryMax;
        job.SalaryUnit = string.IsNullOrWhiteSpace(model.SalaryUnit) ? "VND" : model.SalaryUnit.Trim();
        job.IsSalaryNegotiable = model.IsSalaryNegotiable;
        job.ExperienceLevel = model.ExperienceLevel;
        job.EducationLevel = model.EducationLevel;
        job.Headcount = model.Headcount > 0 ? model.Headcount : 1;
        job.ExpiresAt = model.ExpiresAt.Date.AddDays(1).AddSeconds(-1);
        job.Description = cleanDesc;
        job.Requirements = cleanReq;
        job.UpdatedAt = DateTime.Now;

        // Cập nhật SearchText chuẩn hóa
        var skillNames = await GetSkillNamesAsync(model.SkillIds);
        var companyName = job.Company != null ? job.Company.Name : string.Empty;
        var wp = job.WorkplaceId.HasValue ? await _context.Workplaces.FirstOrDefaultAsync(w => w.WorkplaceId == job.WorkplaceId.Value) : null;
        string wpText = wp != null ? $"{wp.Name} {wp.Address}" : (job.WorkMode == "Remote" ? "Từ xa Remote" : string.Empty);
        job.SearchText = SearchTextBuilder.BuildFromParts(cleanTitle, companyName, skillNames, wpText);

        // Cập nhật kỹ năng
        _context.JobSkills.RemoveRange(job.JobSkills);
        if (model.SkillIds != null && model.SkillIds.Count > 0)
        {
            foreach (var skId in model.SkillIds.Distinct())
            {
                job.JobSkills.Add(new tblJobSkill { JobId = jobId, SkillId = skId });
            }
        }

        // Cập nhật phúc lợi
        _context.JobBenefits.RemoveRange(job.JobBenefits);
        if (model.BenefitIds != null && model.BenefitIds.Count > 0)
        {
            foreach (var bId in model.BenefitIds.Distinct())
            {
                job.JobBenefits.Add(new tblJobBenefit { JobId = jobId, BenefitId = bId });
            }
        }

        await _context.SaveChangesAsync();

        // Ghi AuditLog
        string updateDetails = criticalChanged
            ? (job.ApprovalStatus == "Pending" ? "Sửa thông tin quan trọng, tin chuyển về trạng thái chờ duyệt" : "Sửa thông tin tin tuyển dụng")
            : "Cập nhật tin tuyển dụng";
        await _auditLogService.LogAsync(userId, "UpdateJob", "tblJob", jobId.ToString(), updateDetails);

        string returnMsg = job.ApprovalStatus == "Pending"
            ? "Cập nhật thành công. Do có thay đổi các thông tin quan trọng, tin đã được chuyển sang trạng thái chờ duyệt."
            : "Cập nhật tin tuyển dụng thành công.";

        return (true, returnMsg);
    }

    public async Task<(bool Success, string? Message)> SendToApprovalAsync(int companyId, int userId, int jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);

        if (job == null) return (false, "Không tìm thấy tin tuyển dụng.");

        // AC-S3B.6: Công ty phải Approved
        if (job.Company == null || job.Company.Status != "Approved")
        {
            return (false, "Công ty của bạn chưa được phê duyệt, không thể gửi duyệt tin.");
        }

        // Vòng đời: Chỉ tin Draft mới được gửi duyệt (Draft -> Pending)
        if (job.ApprovalStatus != "Draft")
        {
            return (false, $"Chuyển trạng thái không hợp lệ: Không thể gửi duyệt tin đang ở trạng thái '{job.ApprovalStatus}'.");
        }

        if (job.ExpiresAt <= DateTime.Now)
        {
            return (false, "Hạn nộp hồ sơ của tin đã qua, vui lòng gia hạn trước khi gửi duyệt.");
        }

        if (_listingSettings.AutoApprove)
        {
            job.ApprovalStatus = "Published";
            job.PublishedAt = DateTime.Now;
        }
        else
        {
            job.ApprovalStatus = "Pending";
        }
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        string actionLog = _listingSettings.AutoApprove ? "AutoApproveJob" : "SubmitJobToApproval";
        await _auditLogService.LogAsync(userId, actionLog, "tblJob", jobId.ToString(), "Gửi tin tuyển dụng đi phê duyệt");

        return (true, _listingSettings.AutoApprove ? "Tin tuyển dụng đã được xuất bản thành công." : "Đã gửi tin tuyển dụng đến ban quản trị để phê duyệt.");
    }

    public async Task<(bool Success, string? Message)> CloseJobAsync(int companyId, int userId, int jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);
        if (job == null) return (false, "Không tìm thấy tin tuyển dụng.");

        if (job.ApprovalStatus != "Published")
        {
            return (false, "Chỉ có thể đóng tin tuyển dụng đang hiển thị (Published).");
        }

        job.ApprovalStatus = "Closed";
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync(userId, "CloseJob", "tblJob", jobId.ToString(), "Đóng tin tuyển dụng");

        return (true, "Đã đóng tin tuyển dụng thành công.");
    }

    public async Task<(bool Success, string? Message)> ReopenJobAsync(int companyId, int userId, int jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);
        if (job == null) return (false, "Không tìm thấy tin tuyển dụng.");

        if (job.ApprovalStatus != "Closed" && job.ApprovalStatus != "Expired")
        {
            return (false, "Chỉ có thể mở lại tin tuyển dụng đã đóng hoặc hết hạn.");
        }

        // Kiểm tra hạn nộp
        if (job.ExpiresAt <= DateTime.Now)
        {
            job.ExpiresAt = DateTime.Today.AddDays(30);
        }

        if (_listingSettings.AutoApprove)
        {
            job.ApprovalStatus = "Published";
            job.PublishedAt = DateTime.Now;
        }
        else
        {
            job.ApprovalStatus = "Pending";
        }
        job.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync(userId, "ReopenJob", "tblJob", jobId.ToString(), "Mở lại tin tuyển dụng");

        return (true, _listingSettings.AutoApprove ? "Đã mở lại và xuất bản tin tuyển dụng." : "Đã mở lại và gửi tin đi phê duyệt.");
    }

    public async Task<(bool Success, string? Message, int? NewJobId)> DuplicateJobAsync(int companyId, int userId, int jobId)
    {
        var orig = await _context.Jobs
            .Include(j => j.JobSkills)
            .Include(j => j.JobBenefits)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);

        if (orig == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng cần nhân bản.", null);
        }

        string newTitle = $"{orig.Title} (Bản sao)";
        string newSlug = await GenerateUniqueSlugAsync(newTitle);

        var newJob = new tblJob
        {
            CompanyId = companyId,
            WorkplaceId = orig.WorkplaceId,
            CategoryId = orig.CategoryId,
            JobTypeId = orig.JobTypeId,
            Title = newTitle,
            Slug = newSlug,
            Description = orig.Description,
            Requirements = orig.Requirements,
            SalaryMin = orig.SalaryMin,
            SalaryMax = orig.SalaryMax,
            SalaryUnit = orig.SalaryUnit,
            IsSalaryNegotiable = orig.IsSalaryNegotiable,
            ExperienceLevel = orig.ExperienceLevel,
            EducationLevel = orig.EducationLevel,
            WorkMode = orig.WorkMode,
            Headcount = orig.Headcount,
            ExpiresAt = DateTime.Today.AddDays(30),
            ApprovalStatus = "Draft", // Nhân bản luôn là Draft
            IsFeatured = false,
            ViewCount = 0,
            PublishedAt = null,
            SearchText = orig.SearchText,
            CreatedAt = DateTime.Now
        };

        foreach (var js in orig.JobSkills)
        {
            newJob.JobSkills.Add(new tblJobSkill { SkillId = js.SkillId });
        }

        foreach (var jb in orig.JobBenefits)
        {
            newJob.JobBenefits.Add(new tblJobBenefit { BenefitId = jb.BenefitId });
        }

        _context.Jobs.Add(newJob);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(userId, "DuplicateJob", "tblJob", newJob.JobId.ToString(), $"Nhân bản từ tin ID {jobId}");

        return (true, "Đã nhân bản tin tuyển dụng thành bản nháp mới.", newJob.JobId);
    }

    public async Task<(bool Success, string? Message)> DeleteJobAsync(int companyId, int userId, int jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.JobSkills)
            .Include(j => j.JobBenefits)
            .FirstOrDefaultAsync(j => j.JobId == jobId && j.CompanyId == companyId);

        if (job == null)
        {
            return (false, "Không tìm thấy tin tuyển dụng.");
        }

        // AC-S3B.9: Không cho xóa tin đã có đơn ứng tuyển
        bool hasApplications = await _context.Applications.AnyAsync(a => a.JobId == jobId);
        if (hasApplications)
        {
            return (false, "Không thể xóa tin tuyển dụng đã có ứng viên nộp hồ sơ. Vui lòng chọn đóng tin.");
        }

        _context.JobSkills.RemoveRange(job.JobSkills);
        _context.JobBenefits.RemoveRange(job.JobBenefits);
        _context.Jobs.Remove(job);

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync(userId, "DeleteJob", "tblJob", jobId.ToString(), $"Xóa tin tuyển dụng: {job.Title}");

        return (true, "Đã xóa tin tuyển dụng thành công.");
    }

    // ── HÀM TRỢ GIÚP ────────────────────────────────────────────────────────
    private string? ValidateJobInput(JobInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            return "Tiêu đề tin tuyển dụng không được để trống.";
        }

        if (model.Title.Trim().Length > 250)
        {
            return "Tiêu đề tin tuyển dụng tối đa 250 ký tự.";
        }

        if (!model.IsSalaryNegotiable && model.SalaryMin.HasValue && model.SalaryMax.HasValue)
        {
            if (model.SalaryMin.Value > model.SalaryMax.Value)
            {
                return "Mức lương tối thiểu không được lớn hơn mức lương tối đa.";
            }
        }

        if (model.ExpiresAt.Date < DateTime.Today)
        {
            return "Hạn nộp hồ sơ không được ở trong quá khứ.";
        }

        if (model.ExpiresAt.Date > DateTime.Today.AddDays(90))
        {
            return "Hạn nộp hồ sơ tối đa là 90 ngày kể từ hôm nay.";
        }

        if (model.SkillIds != null && model.SkillIds.Count > 15)
        {
            return "Chỉ được chọn tối đa 15 kỹ năng.";
        }

        if (string.IsNullOrWhiteSpace(model.Description))
        {
            return "Mô tả công việc không được để trống.";
        }

        return null;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? currentJobId = null)
    {
        string baseSlug = SlugHelper.Slugify(title);
        string uniqueSlug = baseSlug;
        int counter = 1;

        while (await _context.Jobs.AnyAsync(j => j.Slug == uniqueSlug && (!currentJobId.HasValue || j.JobId != currentJobId.Value)))
        {
            uniqueSlug = $"{baseSlug}-{counter++}";
        }

        return uniqueSlug;
    }

    private async Task<string> GetSkillNamesAsync(List<int>? skillIds)
    {
        if (skillIds == null || skillIds.Count == 0) return string.Empty;
        var names = await _context.Skills
            .Where(s => skillIds.Contains(s.SkillId))
            .Select(s => s.Name)
            .ToListAsync();
        return string.Join(" ", names);
    }
}

