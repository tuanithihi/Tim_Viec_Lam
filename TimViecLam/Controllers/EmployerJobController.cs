using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Controllers;

/// <summary>
/// Controller quản lý tin tuyển dụng dành cho Nhà tuyển dụng (CRUD, vòng đời, IDOR).
/// </summary>
[Authorize(Policy = "EmployerOnly")]
[Route("nha-tuyen-dung/tin-tuyen-dung")]
public class EmployerJobController : Controller
{
    private readonly IJobManagementService _jobService;
    private readonly ICurrentUserService _currentUser;
    private readonly DataContext _context;

    public EmployerJobController(
        IJobManagementService jobService,
        ICurrentUserService currentUser,
        DataContext context)
    {
        _jobService = jobService;
        _currentUser = currentUser;
        _context = context;
    }

    /// <summary>
    /// Danh sách tin tuyển dụng của công ty (phân trang, lọc theo trạng thái, tìm kiếm).
    /// GET /nha-tuyen-dung/tin-tuyen-dung
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] JobFilterModel filter)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await _jobService.GetEmployerJobsAsync(companyId.Value, filter);
        return View(model);
    }

    /// <summary>
    /// Form đăng tin tuyển dụng mới (chỉ công ty đã được duyệt mới được truy cập).
    /// GET /nha-tuyen-dung/tin-tuyen-dung/them
    /// </summary>
    [Authorize(Policy = "ApprovedEmployer")]
    [HttpGet("them")]
    public async Task<IActionResult> Create()
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        await PopulateDropdownsAsync(companyId.Value);
        var inputModel = await _jobService.GetJobInputForCreateAsync(companyId.Value);
        return View(inputModel ?? new JobInputModel());
    }

    /// <summary>
    /// Xử lý tạo tin tuyển dụng mới.
    /// POST /nha-tuyen-dung/tin-tuyen-dung/them
    /// </summary>
    [Authorize(Policy = "ApprovedEmployer")]
    [HttpPost("them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobInputModel model)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(companyId.Value);
            return View(model);
        }

        var result = await _jobService.CreateJobAsync(companyId.Value, userId.Value, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Tạo tin tuyển dụng thất bại.");
            await PopulateDropdownsAsync(companyId.Value);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Form chỉnh sửa tin tuyển dụng (chống IDOR: không cho sửa tin của công ty khác).
    /// GET /nha-tuyen-dung/tin-tuyen-dung/sua/{id}
    /// </summary>
    [HttpGet("sua/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var inputModel = await _jobService.GetJobInputForEditAsync(companyId.Value, id);
        if (inputModel == null)
        {
            return NotFound("Không tìm thấy tin tuyển dụng hoặc bạn không có quyền chỉnh sửa tin này.");
        }

        await PopulateDropdownsAsync(companyId.Value);
        return View(inputModel);
    }

    /// <summary>
    /// Xử lý cập nhật tin tuyển dụng.
    /// POST /nha-tuyen-dung/tin-tuyen-dung/sua/{id}
    /// </summary>
    [HttpPost("sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobInputModel model)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(companyId.Value);
            return View(model);
        }

        var result = await _jobService.UpdateJobAsync(companyId.Value, userId.Value, id, model);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng.")
            {
                return NotFound(result.Message);
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Cập nhật tin tuyển dụng thất bại.");
            await PopulateDropdownsAsync(companyId.Value);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Gửi tin nháp đi phê duyệt (Draft -> Pending).
    /// POST /nha-tuyen-dung/tin-tuyen-dung/gui-duyet/{id}
    /// </summary>
    [HttpPost("gui-duyet/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToApproval(int id)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _jobService.SendToApprovalAsync(companyId.Value, userId.Value, id);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng.")
            {
                return NotFound(result.Message);
            }
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Đóng tin tuyển dụng đang xuất bản.
    /// POST /nha-tuyen-dung/tin-tuyen-dung/dong-tin/{id}
    /// </summary>
    [HttpPost("dong-tin/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseJob(int id)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _jobService.CloseJobAsync(companyId.Value, userId.Value, id);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng.")
            {
                return NotFound(result.Message);
            }
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Mở lại tin tuyển dụng đã đóng.
    /// POST /nha-tuyen-dung/tin-tuyen-dung/mo-lai/{id}
    /// </summary>
    [HttpPost("mo-lai/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReopenJob(int id)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _jobService.ReopenJobAsync(companyId.Value, userId.Value, id);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng.")
            {
                return NotFound(result.Message);
            }
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Nhân bản tin tuyển dụng thành bản nháp mới.
    /// POST /nha-tuyen-dung/tin-tuyen-dung/nhan-ban/{id}
    /// </summary>
    [HttpPost("nhan-ban/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DuplicateJob(int id)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _jobService.DuplicateJobAsync(companyId.Value, userId.Value, id);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng cần nhân bản.")
            {
                return NotFound(result.Message);
            }
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Edit), new { id = result.NewJobId });
    }

    /// <summary>
    /// Xóa tin tuyển dụng (chặn xóa nếu đã có đơn ứng tuyển, chống IDOR).
    /// POST /nha-tuyen-dung/tin-tuyen-dung/xoa/{id}
    /// </summary>
    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        int? companyId = _currentUser.CompanyId;
        int? userId = _currentUser.UserId;
        if (!companyId.HasValue || !userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _jobService.DeleteJobAsync(companyId.Value, userId.Value, id);
        if (!result.Success)
        {
            if (result.Message == "Không tìm thấy tin tuyển dụng.")
            {
                return NotFound(result.Message);
            }
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ── HÀM TRỢ GIÚP NẠP DROPDOWNS ──────────────────────────────────────────
    private async Task PopulateDropdownsAsync(int companyId)
    {
        ViewBag.Categories = new SelectList(
            await _context.JobCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(),
            "CategoryId", "Name");

        ViewBag.JobTypes = new SelectList(
            await _context.JobTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(),
            "JobTypeId", "Name");

        ViewBag.Workplaces = new SelectList(
            await _context.Workplaces.AsNoTracking().Where(w => w.CompanyId == companyId).OrderByDescending(w => w.IsHeadOffice).ThenBy(w => w.Name).ToListAsync(),
            "WorkplaceId", "Name");

        ViewBag.AllSkills = await _context.Skills.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        ViewBag.AllBenefits = await _context.Benefits.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
    }
}

