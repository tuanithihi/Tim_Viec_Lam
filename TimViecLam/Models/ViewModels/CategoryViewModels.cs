using System.ComponentModel.DataAnnotations;

namespace TimViecLam.Models.ViewModels;

/// <summary>
/// Kết quả phân trang dùng chung cho danh sách dữ liệu.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 10));
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// ViewModel hiển thị Ngành nghề.
/// </summary>
public class JobCategoryViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model nhập liệu Thêm / Sửa Ngành nghề.
/// </summary>
public class JobCategoryInputModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên ngành nghề không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên ngành nghề tối đa 150 ký tự.")]
    [Display(Name = "Tên ngành nghề")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Biểu tượng tối đa 100 ký tự.")]
    [Display(Name = "Biểu tượng Icon (Bootstrap Icons)")]
    public string? Icon { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    [Display(Name = "Mô tả ngành nghề")]
    public string? Description { get; set; }

    [Display(Name = "Thứ tự sắp xếp")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ViewModel hiển thị Hình thức làm việc.
/// </summary>
public class JobTypeViewModel
{
    public int JobTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model nhập liệu Thêm / Sửa Hình thức làm việc.
/// </summary>
public class JobTypeInputModel
{
    public int JobTypeId { get; set; }

    [Required(ErrorMessage = "Tên hình thức làm việc không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên hình thức tối đa 100 ký tự.")]
    [Display(Name = "Tên hình thức")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã hình thức không được để trống.")]
    [StringLength(50, ErrorMessage = "Mã hình thức tối đa 50 ký tự.")]
    [Display(Name = "Mã hình thức")]
    public string Code { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Thứ tự")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ViewModel hiển thị Kỹ năng.
/// </summary>
public class SkillViewModel
{
    public int SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model nhập liệu Thêm / Sửa Kỹ năng.
/// </summary>
public class SkillInputModel
{
    public int SkillId { get; set; }

    [Required(ErrorMessage = "Tên kỹ năng không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên kỹ năng tối đa 100 ký tự.")]
    [Display(Name = "Tên kỹ năng")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ViewModel hiển thị Phúc lợi.
/// </summary>
public class BenefitViewModel
{
    public int BenefitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// Model nhập liệu Thêm / Sửa Phúc lợi.
/// </summary>
public class BenefitInputModel
{
    public int BenefitId { get; set; }

    [Required(ErrorMessage = "Tên phúc lợi không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên phúc lợi tối đa 150 ký tự.")]
    [Display(Name = "Tên phúc lợi")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Biểu tượng tối đa 100 ký tự.")]
    [Display(Name = "Biểu tượng Icon (Bootstrap Icons)")]
    public string? Icon { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO đại diện cho địa giới hành chính (Tỉnh / Quận / Phường) phục vụ API và dropdown.
/// </summary>
public class GeoItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel xem danh sách Tỉnh/Thành tại khu quản trị SuperAdmin.
/// </summary>
public class LocationProvinceViewModel
{
    public int ProvinceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DistrictCount { get; set; }
    public int WorkplaceCount { get; set; }
}

