using System.ComponentModel.DataAnnotations;
using TimViecLam.Models;

namespace TimViecLam.Models.ViewModels;

/// <summary>
/// Model tạo mới hoặc cập nhật Tin tuyển dụng (chống Over-posting: không nhận CompanyId, ApprovalStatus, ViewCount từ client).
/// </summary>
public class JobInputModel
{
    public int JobId { get; set; }

    [Required(ErrorMessage = "Tiêu đề tin tuyển dụng không được để trống.")]
    [StringLength(250, ErrorMessage = "Tiêu đề tin tuyển dụng tối đa 250 ký tự.")]
    [Display(Name = "Tiêu đề việc làm")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngành nghề tuyển dụng.")]
    [Display(Name = "Ngành nghề")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hình thức làm việc.")]
    [Display(Name = "Hình thức làm việc")]
    public int JobTypeId { get; set; }

    [Display(Name = "Địa điểm làm việc")]
    public int? WorkplaceId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chế độ làm việc.")]
    [StringLength(30)]
    [Display(Name = "Chế độ làm việc")]
    public string WorkMode { get; set; } = "OnSite"; // OnSite, Remote, Hybrid

    [Display(Name = "Mức lương tối thiểu")]
    [Range(0, 10000000000, ErrorMessage = "Mức lương tối thiểu không hợp lệ.")]
    public decimal? SalaryMin { get; set; }

    [Display(Name = "Mức lương tối đa")]
    [Range(0, 10000000000, ErrorMessage = "Mức lương tối đa không hợp lệ.")]
    public decimal? SalaryMax { get; set; }

    [StringLength(10)]
    [Display(Name = "Đơn vị tiền tệ")]
    public string SalaryUnit { get; set; } = "VND";

    [Display(Name = "Lương thỏa thuận")]
    public bool IsSalaryNegotiable { get; set; } = false;

    [Required(ErrorMessage = "Vui lòng chọn yêu cầu kinh nghiệm.")]
    [StringLength(50)]
    [Display(Name = "Kinh nghiệm")]
    public string ExperienceLevel { get; set; } = "Không yêu cầu";

    [Required(ErrorMessage = "Vui lòng chọn trình độ học vấn.")]
    [StringLength(50)]
    [Display(Name = "Trình độ học vấn")]
    public string EducationLevel { get; set; } = "Không yêu cầu";

    [Range(1, 1000, ErrorMessage = "Số lượng tuyển dụng từ 1 đến 1000 người.")]
    [Display(Name = "Số lượng tuyển")]
    public int Headcount { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn hạn nộp hồ sơ.")]
    [Display(Name = "Hạn nộp hồ sơ")]
    public DateTime ExpiresAt { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Mô tả công việc không được để trống.")]
    [Display(Name = "Mô tả công việc")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Yêu cầu ứng viên")]
    public string? Requirements { get; set; }

    [Display(Name = "Danh sách kỹ năng")]
    public List<int> SkillIds { get; set; } = new();

    [Display(Name = "Danh sách phúc lợi")]
    public List<int> BenefitIds { get; set; } = new();

    /// <summary>
    /// Nếu true: lưu dưới dạng bản nháp (Draft), không gửi duyệt ngay.
    /// Nếu false: gửi duyệt ngay (chuyển sang Pending hoặc Published nếu AutoApprove).
    /// </summary>
    public bool IsDraft { get; set; } = false;
}

/// <summary>
/// Model bộ lọc và tìm kiếm danh sách tin tuyển dụng.
/// </summary>
public class JobFilterModel
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int? CategoryId { get; set; }
    public int? JobTypeId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO hiển thị từng dòng tin tuyển dụng trong danh sách quản lý của Nhà tuyển dụng.
/// </summary>
public class EmployerJobItemDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public string SalaryDisplay { get; set; } = string.Empty;
    public string WorkMode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int ApplicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsFeatured { get; set; }
    public string? RejectReason { get; set; }
    public bool IsExpired => ExpiresAt < DateTime.Now;
}

/// <summary>
/// ViewModel phân trang danh sách tin tuyển dụng dành cho Nhà tuyển dụng.
/// </summary>
public class EmployerJobListViewModel
{
    public List<EmployerJobItemDto> Jobs { get; set; } = new();
    public JobFilterModel Filter { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 10));
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

/// <summary>
/// DTO hiển thị tin tuyển dụng trong hàng đợi kiểm duyệt của SuperAdmin.
/// </summary>
public class JobModerationItemDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogo { get; set; }
    public string CompanyStatus { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public string SalaryDisplay { get; set; } = string.Empty;
    public string WorkMode { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? RejectReason { get; set; }
}

/// <summary>
/// ViewModel phân trang danh sách duyệt tin dành cho SuperAdmin.
/// </summary>
public class JobModerationListViewModel
{
    public List<JobModerationItemDto> Jobs { get; set; } = new();
    public JobFilterModel Filter { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 10));
    public int PendingCount { get; set; }
}

/// <summary>
/// Chi tiết tin tuyển dụng phục vụ SuperAdmin xem xét trước khi phê duyệt.
/// </summary>
public class JobModerationDetailsViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public string WorkplaceAddress { get; set; } = string.Empty;
    public string WorkMode { get; set; } = string.Empty;
    public string SalaryDisplay { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public int Headcount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? RejectReason { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Benefits { get; set; } = new();

    // Thông tin công ty
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogo { get; set; }
    public string CompanyStatus { get; set; } = string.Empty;
    public string? CompanyWebsite { get; set; }
    public string? CompanyAddress { get; set; }

    // Nhật ký kiểm toán của tin này
    public List<tblAuditLog> AuditLogs { get; set; } = new();
}

/// <summary>
/// Input model gửi lý do khi Từ chối hoặc Gỡ tin tuyển dụng.
/// </summary>
public class RejectJobInputModel
{
    public int JobId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do từ chối hoặc gỡ tin.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Lý do từ 5 đến 500 ký tự.")]
    [Display(Name = "Lý do")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Input model cho hành động duyệt hàng loạt nhiều tin cùng lúc.
/// </summary>
public class BatchApproveInputModel
{
    [Required(ErrorMessage = "Vui lòng chọn ít nhất một tin tuyển dụng.")]
    public List<int> JobIds { get; set; } = new();
}

