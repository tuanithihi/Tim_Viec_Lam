using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Trang quản trị SuperAdmin tạm để kiểm tra phân quyền.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
