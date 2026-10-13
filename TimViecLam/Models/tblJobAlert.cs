using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Thông báo việc làm phù hợp (Job Alert).
/// </summary>
[Table("tblJobAlert")]
public class tblJobAlert
{
    [Key]
    public int AlertId { get; set; }

    [Required]
    [Display(Name = "Mã người dùng")]
    public int UserId { get; set; }

    [StringLength(200)]
    [Display(Name = "Từ khóa tìm kiếm")]
    public string? Keyword { get; set; }

    [Display(Name = "Tỉnh/Thành phố")]
    public int? ProvinceId { get; set; }

    [Display(Name = "Ngành nghề")]
    public int? CategoryId { get; set; }

    [Display(Name = "Mức lương tối thiểu mong muốn")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinSalary { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Tần suất gửi thông báo")]
    public string Frequency { get; set; } = "Daily"; // "Daily" | "Weekly"

    [Display(Name = "Trạng thái kích hoạt")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(UserId))]
    public virtual tblUser? User { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public virtual tblProvince? Province { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual tblJobCategory? Category { get; set; }
}
