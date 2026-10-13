using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Đơn ứng tuyển việc làm (Unique: JobId, CandidateUserId).
/// </summary>
[Table("tblApplication")]
public class tblApplication
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    [Display(Name = "Mã việc làm")]
    public int JobId { get; set; }

    [Required]
    [Display(Name = "Mã ứng viên")]
    public int CandidateUserId { get; set; }

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Thư giới thiệu tối đa 2000 ký tự.")]
    [Display(Name = "Thư giới thiệu / Lời nhắn")]
    public string? CoverLetter { get; set; }

    [Required(ErrorMessage = "Đường dẫn file CV không được để trống.")]
    [StringLength(255, ErrorMessage = "Đường dẫn CV tối đa 255 ký tự.")]
    [Display(Name = "Đường dẫn file CV")]
    public string CvPath { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái ứng tuyển")]
    public string Status { get; set; } = "New"; // "New" | "Viewed" | "Shortlisted" | "Interview" | "Rejected" | "Hired"

    [StringLength(1000)]
    [Display(Name = "Ghi chú của nhà tuyển dụng")]
    public string? EmployerNote { get; set; }

    [Display(Name = "Thời gian nộp đơn")]
    public DateTime AppliedAt { get; set; } = DateTime.Now;

    [Display(Name = "Thời gian cập nhật")]
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(JobId))]
    public virtual tblJob? Job { get; set; }

    [ForeignKey(nameof(CandidateUserId))]
    public virtual tblUser? CandidateUser { get; set; }
}
