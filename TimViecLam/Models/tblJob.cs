using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Tin tuyển dụng / Việc làm.
/// </summary>
[Table("tblJob")]
public class tblJob
{
    [Key]
    public int JobId { get; set; }

    [Required]
    [Display(Name = "Công ty tuyển dụng")]
    public int CompanyId { get; set; }

    [Display(Name = "Địa điểm làm việc")]
    public int? WorkplaceId { get; set; } // Nullable khi làm việc từ xa (Remote)

    [Required]
    [Display(Name = "Ngành nghề")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Hình thức làm việc")]
    public int JobTypeId { get; set; }

    [Required(ErrorMessage = "Tiêu đề tin tuyển dụng không được để trống.")]
    [StringLength(250, ErrorMessage = "Tiêu đề tối đa 250 ký tự.")]
    [Display(Name = "Tiêu đề việc làm")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug không được để trống.")]
    [StringLength(280, ErrorMessage = "Slug tối đa 280 ký tự.")]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả công việc không được để trống.")]
    [Display(Name = "Mô tả công việc")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Yêu cầu ứng viên")]
    public string? Requirements { get; set; }

    [Display(Name = "Mức lương tối thiểu")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryMin { get; set; }

    [Display(Name = "Mức lương tối đa")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryMax { get; set; }

    [Required]
    [StringLength(10)]
    [Display(Name = "Đơn vị tiền tệ")]
    public string SalaryUnit { get; set; } = "VND"; // "VND" | "USD"

    [Display(Name = "Lương thỏa thuận")]
    public bool IsSalaryNegotiable { get; set; } = false;

    [Required]
    [StringLength(50)]
    [Display(Name = "Kinh nghiệm")]
    public string ExperienceLevel { get; set; } = "Không yêu cầu"; // "Không yêu cầu" | "Dưới 1 năm" | "1-2 năm" | "2-5 năm" | "Trên 5 năm"

    [Required]
    [StringLength(50)]
    [Display(Name = "Trình độ học vấn")]
    public string EducationLevel { get; set; } = "Không yêu cầu"; // "Không yêu cầu" | "Trung cấp" | "Cao đẳng" | "Đại học" | "Sau đại học"

    [Required]
    [StringLength(30)]
    [Display(Name = "Chế độ làm việc")]
    public string WorkMode { get; set; } = "OnSite"; // "OnSite" | "Remote" | "Hybrid"

    [Display(Name = "Số lượng tuyển dụng")]
    public int Headcount { get; set; } = 1;

    [Required(ErrorMessage = "Hạn nộp hồ sơ không được để trống.")]
    [Display(Name = "Hạn nộp hồ sơ")]
    public DateTime ExpiresAt { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái kiểm duyệt")]
    public string ApprovalStatus { get; set; } = "Draft"; // "Draft" | "Pending" | "Published" | "Rejected" | "Expired" | "Closed"

    [Display(Name = "Tin nổi bật")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "Lượt xem")]
    public int ViewCount { get; set; } = 0;

    [Display(Name = "Ngày xuất bản")]
    public DateTime? PublishedAt { get; set; }

    [StringLength(500)]
    [Display(Name = "Lý do từ chối")]
    public string? RejectReason { get; set; }

    [Required]
    [StringLength(2000)]
    [Display(Name = "Chuỗi tìm kiếm chuẩn hóa")]
    public string SearchText { get; set; } = string.Empty;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(CompanyId))]
    public virtual tblCompany? Company { get; set; }

    [ForeignKey(nameof(WorkplaceId))]
    public virtual tblWorkplace? Workplace { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual tblJobCategory? Category { get; set; }

    [ForeignKey(nameof(JobTypeId))]
    public virtual tblJobType? JobType { get; set; }

    public virtual ICollection<tblJobSkill> JobSkills { get; set; } = new List<tblJobSkill>();
    public virtual ICollection<tblJobBenefit> JobBenefits { get; set; } = new List<tblJobBenefit>();
    public virtual ICollection<tblApplication> Applications { get; set; } = new List<tblApplication>();
    public virtual ICollection<tblSavedJob> SavedJobs { get; set; } = new List<tblSavedJob>();
}
