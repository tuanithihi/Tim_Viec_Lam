using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Ngành nghề việc làm.
/// </summary>
[Table("tblJobCategory")]
public class tblJobCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên ngành nghề không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên ngành nghề tối đa 150 ký tự.")]
    [Display(Name = "Tên ngành nghề")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đường dẫn (Slug) không được để trống.")]
    [StringLength(160, ErrorMessage = "Đường dẫn tối đa 160 ký tự.")]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Biểu tượng (Icon CSS)")]
    public string? Icon { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả ngành nghề")]
    public string? Description { get; set; }

    [Display(Name = "Thứ tự sắp xếp")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ICollection<tblJob> Jobs { get; set; } = new List<tblJob>();
    public virtual ICollection<tblJobAlert> JobAlerts { get; set; } = new List<tblJobAlert>();
}
