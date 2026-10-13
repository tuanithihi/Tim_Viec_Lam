using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Chế độ phúc lợi việc làm (Bảo hiểm, thưởng, laptop, du lịch...).
/// </summary>
[Table("tblBenefit")]
public class tblBenefit
{
    [Key]
    public int BenefitId { get; set; }

    [Required(ErrorMessage = "Tên phúc lợi không được để trống.")]
    [StringLength(150)]
    [Display(Name = "Tên phúc lợi")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Biểu tượng (Icon CSS)")]
    public string? Icon { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ICollection<tblJobBenefit> JobBenefits { get; set; } = new List<tblJobBenefit>();
}
