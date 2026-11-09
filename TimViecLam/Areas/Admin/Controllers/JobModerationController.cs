using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller kiểm duyệt tin tuyển dụng dành riêng cho SuperAdmin (Duyệt, Từ chối, Gỡ tin, Duyệt hàng loạt).
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
[Route("admin/duyet-tin")]
public class JobModerationController : Controller
{
    private readonly IJobModerationService _moderationService;
    private readonly ICurrentUserService _currentUser;

    public JobModerationController(
        IJobModerationService moderationService,
        ICurrentUserService currentUser)
    {
        _moderationService = moderationService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Danh sách tin tuyển dụng chờ duyệt hoặc theo bộ lọc (phân trang, tìm kiếm).
    /// GET /admin/duyet-tin
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] JobFilterModel filter)
    {
        var model = await _moderationService.GetPendingJobsAsync(filter);
        return View(model);
    }

    /// <summary>
    /// Chi tiết tin tuyển dụng phục vụ xem xét và kiểm duyệt.
    /// GET /admin/duyet-tin/chi-tiet/{id}
    /// </summary>
    [HttpGet("chi-tiet/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var job = await _moderationService.GetJobDetailsForModerationAsync(id);
        if (job == null)
        {
            return NotFound("Không tìm thấy tin tuyển dụng.");
        }

        return View(job);
    }

    /// <summary>
    /// Phê duyệt và xuất bản tin tuyển dụng (chỉ tin Pending mới được duyệt).
    /// POST /admin/duyet-tin/duyet/{id}
    /// </summary>
    [HttpPost("duyet/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        int adminUserId = _currentUser.UserId ?? 1;
        var result = await _moderationService.ApproveJobAsync(adminUserId, id);

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
    /// Từ chối tin tuyển dụng (bắt buộc nhập lý do).
    /// POST /admin/duyet-tin/tu-choi/{id}
    /// </summary>
    [HttpPost("tu-choi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, [FromForm] string reason)
    {
        int adminUserId = _currentUser.UserId ?? 1;
        var result = await _moderationService.RejectJobAsync(adminUserId, id, reason);

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
    /// Gỡ tin tuyển dụng đang xuất bản (bắt buộc nhập lý do).
    /// POST /admin/duyet-tin/go-tin/{id}
    /// </summary>
    [HttpPost("go-tin/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id, [FromForm] string reason)
    {
        int adminUserId = _currentUser.UserId ?? 1;
        var result = await _moderationService.UnpublishJobAsync(adminUserId, id, reason);

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
    /// Phê duyệt hàng loạt nhiều tin tuyển dụng cùng lúc (chỉ xử lý các tin Pending).
    /// POST /admin/duyet-tin/duyet-hang-loat
    /// </summary>
    [HttpPost("duyet-hang-loat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchApprove([FromForm] List<int> jobIds)
    {
        int adminUserId = _currentUser.UserId ?? 1;
        var result = await _moderationService.BatchApproveJobsAsync(adminUserId, jobIds);

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

