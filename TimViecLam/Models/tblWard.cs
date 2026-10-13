using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Phường / Xã / Thị trấn.
/// </summary>
[Table("tblWard")]
public class tblWard
{
    [Key]
    public int WardId { get; set; }

    [Required]
    [Display(Name = "Quận/Huyện")]
    public int DistrictId { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã phường/xã")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên phường/xã không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Tên phường/xã")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Tên đầy đủ")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(DistrictId))]
    public virtual tblDistrict? District { get; set; }

    public virtual ICollection<tblWorkplace> Workplaces { get; set; } = new List<tblWorkplace>();
}
