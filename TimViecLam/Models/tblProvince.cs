using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Tỉnh / Thành phố trực thuộc trung ương.
/// </summary>
[Table("tblProvince")]
public class tblProvince
{
    [Key]
    public int ProvinceId { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã tỉnh/thành")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tỉnh/thành không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Tên tỉnh/thành")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Tên đầy đủ")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ICollection<tblDistrict> Districts { get; set; } = new List<tblDistrict>();
    public virtual ICollection<tblWorkplace> Workplaces { get; set; } = new List<tblWorkplace>();
    public virtual ICollection<tblCandidateProfile> CandidateProfiles { get; set; } = new List<tblCandidateProfile>();
}
