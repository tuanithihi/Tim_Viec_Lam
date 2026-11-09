using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller tra cứu dữ liệu địa giới hành chính (Tỉnh, Quận, Phường) cho SuperAdmin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class LocationController : Controller
{
    private readonly IGeoService _geoService;

    public LocationController(IGeoService geoService)
    {
        _geoService = geoService;
    }

    // GET: /Admin/Location
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _geoService.GetPagedProvincesAsync(search, page, 15);
        ViewBag.Search = search ?? string.Empty;
        return View(result);
    }
}

