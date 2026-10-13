using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Địa điểm làm việc / Chi nhánh của công ty.
/// </summary>
[Table("tblWorkplace")]
public class tblWorkplace
{
    [Key]
    public int WorkplaceId { get; set; }

    [Required]
    [Display(Name = "Mã công ty")]
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Tên địa điểm làm việc không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên địa điểm tối đa 150 ký tự.")]
    [Display(Name = "Tên địa điểm / chi nhánh")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống.")]
    [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
    [Display(Name = "Địa chỉ chi tiết")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
    [Display(Name = "Tỉnh/Thành phố")]
    public int ProvinceId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
    [Display(Name = "Quận/Huyện")]
    public int DistrictId { get; set; }

    [Display(Name = "Phường/Xã")]
    public int? WardId { get; set; }

    [Display(Name = "Vĩ độ")]
    public double? Latitude { get; set; }

    [Display(Name = "Kinh độ")]
    public double? Longitude { get; set; }

    [Display(Name = "Trụ sở chính")]
    public bool IsHeadOffice { get; set; } = false;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ── Navigation Properties ───────────────────────────────────────────
    [ForeignKey(nameof(CompanyId))]
    public virtual tblCompany? Company { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public virtual tblProvince? Province { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public virtual tblDistrict? District { get; set; }

    [ForeignKey(nameof(WardId))]
    public virtual tblWard? Ward { get; set; }

    public virtual ICollection<tblJob> Jobs { get; set; } = new List<tblJob>();
}
