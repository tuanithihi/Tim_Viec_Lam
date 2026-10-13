using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Công ty / Nhà tuyển dụng.
/// </summary>
[Table("tblCompany")]
public class tblCompany
{
    [Key]
    public int CompanyId { get; set; }

    [Required]
    [Display(Name = "Mã người dùng đại diện")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Tên công ty không được để trống.")]
    [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự.")]
    [Display(Name = "Tên công ty")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đường dẫn định danh (Slug) không được để trống.")]
    [StringLength(220, ErrorMessage = "Đường dẫn tối đa 220 ký tự.")]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Đường dẫn logo tối đa 255 ký tự.")]
    [Display(Name = "Logo công ty")]
    public string? Logo { get; set; }

    [StringLength(255, ErrorMessage = "Địa chỉ website tối đa 255 ký tự.")]
    [Url(ErrorMessage = "Website không đúng định dạng URL.")]
    [Display(Name = "Trang web")]
    public string? Website { get; set; }

    [StringLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự.")]
    [Display(Name = "Mã số thuế")]
    public string? TaxCode { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Quy mô công ty")]
    public string CompanySize { get; set; } = "Small"; // "Micro" | "Small" | "Medium" | "Large" | "Enterprise"

    [Display(Name = "Mô tả công ty")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Email liên hệ tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email liên hệ")]
    public string? Email { get; set; }

    [StringLength(300, ErrorMessage = "Địa chỉ trụ sở tối đa 300 ký tự.")]
    [Display(Name = "Địa chỉ trụ sở")]
    public string? Address { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái phê duyệt")]
    public string Status { get; set; } = "Pending"; // "Pending" | "Approved" | "Suspended" | "Rejected"

    [StringLength(500, ErrorMessage = "Lý do từ chối tối đa 500 ký tự.")]
    [Display(Name = "Lý do từ chối")]
    public string? RejectReason { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(UserId))]
    public virtual tblUser? User { get; set; }

    public virtual ICollection<tblWorkplace> Workplaces { get; set; } = new List<tblWorkplace>();
    public virtual ICollection<tblJob> Jobs { get; set; } = new List<tblJob>();
}
