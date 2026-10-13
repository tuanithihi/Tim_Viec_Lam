using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Quận / Huyện / Thị xã.
/// </summary>
[Table("tblDistrict")]
public class tblDistrict
{
    [Key]
    public int DistrictId { get; set; }

    [Required]
    [Display(Name = "Tỉnh/Thành")]
    public int ProvinceId { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã quận/huyện")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên quận/huyện không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Tên quận/huyện")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Tên đầy đủ")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(ProvinceId))]
    public virtual tblProvince? Province { get; set; }

    public virtual ICollection<tblWard> Wards { get; set; } = new List<tblWard>();
    public virtual ICollection<tblWorkplace> Workplaces { get; set; } = new List<tblWorkplace>();
}
