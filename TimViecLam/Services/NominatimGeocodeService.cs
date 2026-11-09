using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ gọi proxy OpenStreetMap Nominatim: có cache 24h, giới hạn tối đa 1 req/giây toàn hệ thống,
/// gửi User-Agent theo quy định và timeout 5 giây.
/// </summary>
public class NominatimGeocodeService : INominatimGeocodeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NominatimGeocodeService> _logger;

    private static readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private static DateTime _lastRequestTime = DateTime.MinValue;

    private class NominatimPlaceDto
    {
        public string? display_name { get; set; }
        public string? lat { get; set; }
        public string? lon { get; set; }
    }

    public NominatimGeocodeService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<NominatimGeocodeService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;

        // Cấu hình timeout và User-Agent nếu chưa có
        if (_httpClient.Timeout > TimeSpan.FromSeconds(5) || _httpClient.Timeout == TimeSpan.FromSeconds(100))
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TimViecLam-JobPortal/1.0 (contact@timvieclam.local)");
        }
    }

    public async Task<(bool Success, string? Message, List<GeocodeResultDto> Results)> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (false, "Chuỗi địa chỉ tìm kiếm không được để trống.", new List<GeocodeResultDto>());
        }

        string normalizedQuery = query.Trim().ToLowerInvariant();
        string cacheKey = $"geocode_{normalizedQuery}";

        // 1. Kiểm tra cache: nếu có trong cache thì trả về ngay, không gọi ra ngoài
        if (_cache.TryGetValue(cacheKey, out List<GeocodeResultDto>? cachedResults) && cachedResults != null)
        {
            _logger.LogInformation("Lấy kết quả geocode từ cache cho: {Query}", query);
            return (true, null, cachedResults);
        }

        // 2. Kiểm soát tốc độ gọi: tối đa 1 request / giây toàn hệ thống
        await _rateLimiter.WaitAsync(cancellationToken);
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequestTime;
            if (elapsed < TimeSpan.FromSeconds(1))
            {
                var delay = TimeSpan.FromSeconds(1) - elapsed;
                await Task.Delay(delay, cancellationToken);
            }
            _lastRequestTime = DateTime.UtcNow;

            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&countrycodes=vn&limit=5&addressdetails=1";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim trả về mã lỗi HTTP: {StatusCode}", response.StatusCode);
                return (false, "Dịch vụ bản đồ phản hồi lỗi.", new List<GeocodeResultDto>());
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            var places = JsonSerializer.Deserialize<List<NominatimPlaceDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var results = new List<GeocodeResultDto>();
            if (places != null)
            {
                foreach (var p in places)
                {
                    if (double.TryParse(p.lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(p.lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                    {
                        results.Add(new GeocodeResultDto
                        {
                            DisplayName = p.display_name ?? string.Empty,
                            Latitude = lat,
                            Longitude = lon
                        });
                    }
                }
            }

            // Lưu vào cache 24 giờ
            _cache.Set(cacheKey, results, TimeSpan.FromHours(24));

            return (true, null, results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Geocode request bị quá thời gian chờ (timeout 5s) cho truy vấn: {Query}", query);
            return (false, "Quá thời gian kết nối đến máy chủ bản đồ (timeout 5 giây). Vui lòng thử lại sau.", new List<GeocodeResultDto>());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi mạng khi kết nối đến Nominatim cho truy vấn: {Query}", query);
            return (false, "Không thể kết nối đến máy chủ bản đồ địa lý.", new List<GeocodeResultDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi xử lý geocode: {Query}", query);
            return (false, "Đã xảy ra sự cố khi xử lý dữ liệu địa chỉ.", new List<GeocodeResultDto>());
        }
        finally
        {
            _rateLimiter.Release();
        }
    }
}

