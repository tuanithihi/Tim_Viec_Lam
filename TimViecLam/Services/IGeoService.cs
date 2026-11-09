using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ dữ liệu địa giới hành chính (Tỉnh, Quận, Phường) kèm bộ nhớ đệm.
/// </summary>
public interface IGeoService
{
    /// <summary>Lấy danh sách toàn bộ 63 Tỉnh/Thành phố.</summary>
    Task<List<GeoItemDto>> GetProvincesAsync();

    /// <summary>Lấy danh sách Quận/Huyện theo mã Tỉnh/Thành.</summary>
    Task<List<GeoItemDto>> GetDistrictsByProvinceIdAsync(int provinceId);

    /// <summary>Lấy danh sách Phường/Xã theo mã Quận/Huyện.</summary>
    Task<List<GeoItemDto>> GetWardsByDistrictIdAsync(int districtId);

    /// <summary>Lấy danh sách phân trang Tỉnh/Thành phố cho trang quản trị.</summary>
    Task<PagedResult<LocationProvinceViewModel>> GetPagedProvincesAsync(string? search, int page, int pageSize);
}

