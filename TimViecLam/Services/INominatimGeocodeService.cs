using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ định vị tọa độ địa lý thông qua Nominatim OpenStreetMap (có cache và giới hạn tần suất).
/// </summary>
public interface INominatimGeocodeService
{
    /// <summary>
    /// Tìm kiếm danh sách tọa độ (Latitude, Longitude) từ chuỗi địa chỉ văn bản.
    /// </summary>
    Task<(bool Success, string? Message, List<GeocodeResultDto> Results)> SearchAsync(string query, CancellationToken cancellationToken = default);
}
