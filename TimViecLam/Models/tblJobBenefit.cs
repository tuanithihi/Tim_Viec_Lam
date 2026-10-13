using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Liên kết Việc làm - Phúc lợi (Khóa chính kép: JobId, BenefitId).
/// </summary>
[Table("tblJobBenefit")]
public class tblJobBenefit
{
    [Required]
    [Display(Name = "Mã việc làm")]
    public int JobId { get; set; }

    [Required]
    [Display(Name = "Mã phúc lợi")]
    public int BenefitId { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(JobId))]
    public virtual tblJob? Job { get; set; }

    [ForeignKey(nameof(BenefitId))]
    public virtual tblBenefit? Benefit { get; set; }
}
