using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Kỹ năng chuyên môn.
/// </summary>
[Table("tblSkill")]
public class tblSkill
{
    [Key]
    public int SkillId { get; set; }

    [Required(ErrorMessage = "Tên kỹ năng không được để trống.")]
    [StringLength(100)]
    [Display(Name = "Tên kỹ năng")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug kỹ năng không được để trống.")]
    [StringLength(120)]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ICollection<tblJobSkill> JobSkills { get; set; } = new List<tblJobSkill>();
}
