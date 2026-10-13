using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Controllers;

/// <summary>Controller trang chủ và các trang tĩnh cơ bản.</summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>Khởi tạo HomeController với logger.</summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>Trang chủ - hiển thị ô tìm kiếm tạm.</summary>
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "TimViecLam - Sàn Tìm Việc Làm Hàng Đầu Việt Nam";
        ViewData["Description"] = "Tìm kiếm việc làm nhanh chóng theo vị trí, địa điểm, mức lương. Hàng nghìn tin tuyển dụng từ các nhà tuyển dụng uy tín.";
        return View();
    }

    /// <summary>Trang lỗi 500 - không lộ stack trace.</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>Trang từ chối truy cập (403).</summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Từ chối truy cập";
        Response.StatusCode = 403;
        return View();
    }

    /// <summary>Trang không tìm thấy (404) - dùng trong UseStatusCodePagesWithReExecute.</summary>
    [HttpGet]
    public new IActionResult NotFound()
    {
        ViewData["Title"] = "Không tìm thấy trang";
        Response.StatusCode = 404;
        return View();
    }
}
