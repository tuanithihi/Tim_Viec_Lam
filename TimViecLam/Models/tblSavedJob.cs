using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Việc làm đã lưu của ứng viên (Unique: UserId, JobId).
/// </summary>
[Table("tblSavedJob")]
public class tblSavedJob
{
    [Key]
    public int SavedJobId { get; set; }

    [Required]
    [Display(Name = "Mã người dùng")]
    public int UserId { get; set; }

    [Required]
    [Display(Name = "Mã việc làm")]
    public int JobId { get; set; }

    [Display(Name = "Thời gian lưu")]
    public DateTime SavedAt { get; set; } = DateTime.Now;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(UserId))]
    public virtual tblUser? User { get; set; }

    [ForeignKey(nameof(JobId))]
    public virtual tblJob? Job { get; set; }
}
