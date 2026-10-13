using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Hình thức làm việc (Toàn thời gian, Bán thời gian, Thực tập, Hợp đồng, Freelance).
/// </summary>
[Table("tblJobType")]
public class tblJobType
{
    [Key]
    public int JobTypeId { get; set; }

    [Required(ErrorMessage = "Tên hình thức làm việc không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Tên hình thức")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Mã hình thức")]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Thứ tự")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ICollection<tblJob> Jobs { get; set; } = new List<tblJob>();
}
