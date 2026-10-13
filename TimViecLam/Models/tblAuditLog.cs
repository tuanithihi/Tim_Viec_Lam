using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Nhật ký kiểm toán / theo dõi thao tác người dùng và quản trị viên.
/// </summary>
[Table("tblAuditLog")]
public class tblAuditLog
{
    [Key]
    public int LogId { get; set; }

    [Display(Name = "Mã người thực hiện")]
    public int? UserId { get; set; }

    [Required(ErrorMessage = "Hành động không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Hành động (Action)")]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên bảng / Đối tượng")]
    public string EntityName { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Mã đối tượng (Entity ID)")]
    public string? EntityId { get; set; }

    [StringLength(2000)]
    [Display(Name = "Chi tiết / Lý do thay đổi")]
    public string? Details { get; set; }

    [StringLength(50)]
    [Display(Name = "Địa chỉ IP")]
    public string? IpAddress { get; set; }

    [Display(Name = "Thời gian ghi nhận")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(UserId))]
    public virtual tblUser? User { get; set; }
}
