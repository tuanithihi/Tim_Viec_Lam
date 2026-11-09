using System.ComponentModel.DataAnnotations;

namespace TimViecLam.Models.ViewModels;

/// <summary>
/// DTO kết quả tìm kiếm tọa độ từ dịch vụ địa lý Geocode.
/// </summary>
public class GeocodeResultDto
{
    public string DisplayName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// ViewModel hiển thị hồ sơ công ty cho Nhà tuyển dụng.
/// </summary>
public class CompanyProfileViewModel
{
    public int CompanyId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? TaxCode { get; set; }
    public string CompanySize { get; set; } = "Small";
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int WorkplaceCount { get; set; }
}

/// <summary>
/// Model cập nhật hồ sơ công ty (chống Over-posting: không chứa CompanyId, UserId, Status, RejectReason).
/// </summary>
public class CompanyProfileInputModel
{
    [Required(ErrorMessage = "Tên công ty không được để trống.")]
    [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự.")]
    [Display(Name = "Tên công ty")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Website tối đa 255 ký tự.")]
    [Url(ErrorMessage = "Địa chỉ website không hợp lệ.")]
    [Display(Name = "Trang web")]
    public string? Website { get; set; }

    [StringLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự.")]
    [Display(Name = "Mã số thuế")]
    public string? TaxCode { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quy mô công ty.")]
    [StringLength(50)]
    [Display(Name = "Quy mô công ty")]
    public string CompanySize { get; set; } = "Small";

    [Display(Name = "Mô tả giới thiệu công ty")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại liên hệ")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email liên hệ")]
    public string? Email { get; set; }

    [StringLength(300, ErrorMessage = "Địa chỉ trụ sở tối đa 300 ký tự.")]
    [Display(Name = "Địa chỉ trụ sở chính")]
    public string? Address { get; set; }
}

/// <summary>
/// ViewModel quản trị công ty dành cho SuperAdmin.
/// </summary>
public class CompanyAdminViewModel
{
    public int CompanyId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? TaxCode { get; set; }
    public string CompanySize { get; set; } = "Small";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public int WorkplaceCount { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model nhận lý do từ chối hoặc tạm khóa công ty.
/// </summary>
public class CompanyStatusActionModel
{
    [Required(ErrorMessage = "Vui lòng nhập lý do cụ thể.")]
    [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự.")]
    [Display(Name = "Lý do")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel hiển thị Địa điểm làm việc / Chi nhánh.
/// </summary>
public class WorkplaceViewModel
{
    public int WorkplaceId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public int? WardId { get; set; }
    public string? WardName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsHeadOffice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model tạo/sửa Địa điểm làm việc (chống Over-posting: không nhận CompanyId từ client).
/// </summary>
public class WorkplaceInputModel
{
    public int WorkplaceId { get; set; }

    [Required(ErrorMessage = "Tên địa điểm không được để trống.")]
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

    [Display(Name = "Vĩ độ (Latitude)")]
    public double? Latitude { get; set; }

    [Display(Name = "Kinh độ (Longitude)")]
    public double? Longitude { get; set; }

    [Display(Name = "Là trụ sở chính")]
    public bool IsHeadOffice { get; set; }
}

