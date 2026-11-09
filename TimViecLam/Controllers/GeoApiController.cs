using Microsoft.AspNetCore.Mvc;
using TimViecLam.Services;

namespace TimViecLam.Controllers;

/// <summary>
/// API địa lý công khai phục vụ dropdown phụ thuộc Tỉnh/Thành -> Quận/Huyện -> Phường/Xã.
/// </summary>
[ApiController]
[Route("api/geo")]
public class GeoApiController : ControllerBase
{
    private readonly IGeoService _geoService;

    public GeoApiController(IGeoService geoService)
    {
        _geoService = geoService;
    }

    /// <summary>
    /// Lấy danh sách 63 Tỉnh/Thành phố toàn quốc.
    /// GET /api/geo/provinces
    /// </summary>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        Response.Headers.CacheControl = "public, max-age=3600";
        var provinces = await _geoService.GetProvincesAsync();
        return Ok(provinces);
    }

    /// <summary>
    /// Lấy danh sách Quận/Huyện theo provinceId.
    /// GET /api/geo/districts?provinceId=1
    /// </summary>
    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int? provinceId)
    {
        if (!provinceId.HasValue || provinceId.Value <= 0)
        {
            return BadRequest(new { message = "Tham số provinceId không hợp lệ hoặc bị thiếu." });
        }

        Response.Headers.CacheControl = "public, max-age=3600";
        var districts = await _geoService.GetDistrictsByProvinceIdAsync(provinceId.Value);
        return Ok(districts);
    }

    /// <summary>
    /// Lấy danh sách Phường/Xã theo districtId.
    /// GET /api/geo/wards?districtId=1
    /// </summary>
    [HttpGet("wards")]
    public async Task<IActionResult> GetWards([FromQuery] int? districtId)
    {
        if (!districtId.HasValue || districtId.Value <= 0)
        {
            return BadRequest(new { message = "Tham số districtId không hợp lệ hoặc bị thiếu." });
        }

        Response.Headers.CacheControl = "public, max-age=3600";
        var wards = await _geoService.GetWardsByDistrictIdAsync(districtId.Value);
        return Ok(wards);
    }
}

