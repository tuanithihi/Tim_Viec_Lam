using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể người dùng hệ thống (Candidate, Employer, SuperAdmin).
/// </summary>
[Table("tblUser")]
public class tblUser
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(256)]
    [Display(Name = "Mật khẩu băm")]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string? FullName { get; set; }

    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(255, ErrorMessage = "Đường dẫn ảnh đại diện tối đa 255 ký tự.")]
    [Display(Name = "Ảnh đại diện")]
    public string? Avatar { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Candidate"; // "SuperAdmin" | "Employer" | "Candidate"

    [Display(Name = "Trạng thái kích hoạt")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Số lần đăng nhập sai")]
    public int FailedLoginCount { get; set; } = 0;

    [Display(Name = "Thời gian khóa tài khoản")]
    public DateTime? LockoutEnd { get; set; }

    [StringLength(256)]
    [Display(Name = "Bản băm token đặt lại mật khẩu")]
    public string? ResetTokenHash { get; set; }

    [Display(Name = "Thời gian hết hạn token đặt lại mật khẩu")]
    public DateTime? ResetTokenExpiresAt { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual tblCompany? Company { get; set; }
    public virtual tblCandidateProfile? CandidateProfile { get; set; }
    public virtual ICollection<tblApplication> Applications { get; set; } = new List<tblApplication>();
    public virtual ICollection<tblSavedJob> SavedJobs { get; set; } = new List<tblSavedJob>();
    public virtual ICollection<tblJobAlert> JobAlerts { get; set; } = new List<tblJobAlert>();
    public virtual ICollection<tblAuditLog> AuditLogs { get; set; } = new List<tblAuditLog>();
}
