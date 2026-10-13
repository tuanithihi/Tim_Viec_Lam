using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Liên kết Việc làm - Kỹ năng (Khóa chính kép: JobId, SkillId).
/// </summary>
[Table("tblJobSkill")]
public class tblJobSkill
{
    [Required]
    [Display(Name = "Mã việc làm")]
    public int JobId { get; set; }

    [Required]
    [Display(Name = "Mã kỹ năng")]
    public int SkillId { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(JobId))]
    public virtual tblJob? Job { get; set; }

    [ForeignKey(nameof(SkillId))]
    public virtual tblSkill? Skill { get; set; }
}
