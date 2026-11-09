using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller kiểm duyệt và quản lý danh sách công ty dành cho SuperAdmin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
[Route("Admin/[controller]")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Hiển thị danh sách công ty, hỗ trợ tìm kiếm, lọc theo trạng thái và phân trang.
    /// GET /Admin/Company
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1)
    {
        var result = await _companyService.GetPagedCompaniesAsync(search, status, page, 10);
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(result);
    }

    /// <summary>
    /// Xem chi tiết hồ sơ công ty.
    /// GET /Admin/Company/Details/5
    /// </summary>
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            return NotFound("Không tìm thấy công ty.");
        }

        return View(company);
    }

    /// <summary>
    /// Phê duyệt công ty sang trạng thái Approved.
    /// POST /Admin/Company/Approve/5
    /// </summary>
    [HttpPost("Approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _companyService.ApproveCompanyAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Từ chối phê duyệt công ty (bắt buộc nhập lý do).
    /// POST /Admin/Company/Reject/5
    /// </summary>
    [HttpPost("Reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, [FromForm] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Lý do từ chối không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _companyService.RejectCompanyAsync(id, reason);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Tạm khóa công ty (bắt buộc nhập lý do).
    /// POST /Admin/Company/Suspend/5
    /// </summary>
    [HttpPost("Suspend/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(int id, [FromForm] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Lý do tạm khóa không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _companyService.SuspendCompanyAsync(id, reason);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Mở khóa lại công ty sang trạng thái Approved.
    /// POST /Admin/Company/Unsuspend/5
    /// </summary>
    [HttpPost("Unsuspend/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsuspend(int id)
    {
        var result = await _companyService.UnsuspendCompanyAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
