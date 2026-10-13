using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Hồ sơ năng lực của ứng viên (Quan hệ 1-1 với tblUser).
/// </summary>
[Table("tblCandidateProfile")]
public class tblCandidateProfile
{
    [Key]
    public int ProfileId { get; set; }

    [Required]
    [Display(Name = "Mã người dùng")]
    public int UserId { get; set; }

    [StringLength(200, ErrorMessage = "Tiêu đề hồ sơ tối đa 200 ký tự.")]
    [Display(Name = "Tiêu đề hồ sơ (Headline)")]
    public string? Headline { get; set; }

    [StringLength(200, ErrorMessage = "Vị trí mong muốn tối đa 200 ký tự.")]
    [Display(Name = "Vị trí mong muốn")]
    public string? DesiredPosition { get; set; }

    [Display(Name = "Mức lương mong muốn")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedSalary { get; set; }

    [Display(Name = "Tỉnh/Thành phố sinh sống")]
    public int? ProvinceId { get; set; }

    [Display(Name = "Tóm tắt bản thân")]
    public string? Summary { get; set; }

    [StringLength(255, ErrorMessage = "Đường dẫn CV mặc định tối đa 255 ký tự.")]
    [Display(Name = "Đường dẫn CV mặc định")]
    public string? DefaultCvPath { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(UserId))]
    public virtual tblUser? User { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public virtual tblProvince? Province { get; set; }
}
