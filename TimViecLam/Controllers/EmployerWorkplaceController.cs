using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Controllers;

/// <summary>
/// Controller quản lý địa điểm làm việc / chi nhánh dành cho Nhà tuyển dụng (cô lập dữ liệu theo công ty, chống IDOR).
/// </summary>
[Authorize(Policy = "EmployerOnly")]
[Route("nha-tuyen-dung/dia-diem")]
public class EmployerWorkplaceController : Controller
{
    private readonly IWorkplaceService _workplaceService;
    private readonly ICurrentUserService _currentUser;
    private readonly IGeoService _geoService;

    public EmployerWorkplaceController(
        IWorkplaceService workplaceService,
        ICurrentUserService currentUser,
        IGeoService geoService)
    {
        _workplaceService = workplaceService;
        _currentUser = currentUser;
        _geoService = geoService;
    }

    /// <summary>
    /// Danh sách địa điểm làm việc của công ty hiện tại (hỗ trợ tìm kiếm, phân trang).
    /// GET /nha-tuyen-dung/dia-diem
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? search, [FromQuery] int page = 1)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var pagedResult = await _workplaceService.GetPagedWorkplacesAsync(companyId.Value, search, page, 10);
        ViewBag.Search = search;
        return View(pagedResult);
    }

    /// <summary>
    /// Hiển thị form thêm mới địa điểm làm việc.
    /// GET /nha-tuyen-dung/dia-diem/them
    /// </summary>
    [HttpGet("them")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Provinces = await _geoService.GetProvincesAsync();
        return View(new WorkplaceInputModel());
    }

    /// <summary>
    /// Thêm mới địa điểm làm việc (CompanyId lấy từ session, chống Over-posting).
    /// POST /nha-tuyen-dung/dia-diem/them
    /// </summary>
    [HttpPost("them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkplaceInputModel model)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Provinces = await _geoService.GetProvincesAsync();
            return View(model);
        }

        var result = await _workplaceService.CreateWorkplaceAsync(companyId.Value, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Thêm địa điểm thất bại.");
            ViewBag.Provinces = await _geoService.GetProvincesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Hiển thị form chỉnh sửa địa điểm làm việc (Chống IDOR: không cho xem địa điểm của công ty khác).
    /// GET /nha-tuyen-dung/dia-diem/sua/{id}
    /// </summary>
    [HttpGet("sua/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var workplace = await _workplaceService.GetWorkplaceByIdAsync(id, companyId.Value);
        if (workplace == null)
        {
            // Trả về 404 NotFound khi không tìm thấy hoặc địa điểm thuộc công ty khác (chống IDOR)
            return NotFound("Không tìm thấy địa điểm làm việc.");
        }

        var model = new WorkplaceInputModel
        {
            WorkplaceId = workplace.WorkplaceId,
            Name = workplace.Name,
            Address = workplace.Address,
            ProvinceId = workplace.ProvinceId,
            DistrictId = workplace.DistrictId,
            WardId = workplace.WardId,
            Latitude = workplace.Latitude,
            Longitude = workplace.Longitude,
            IsHeadOffice = workplace.IsHeadOffice
        };

        ViewBag.Provinces = await _geoService.GetProvincesAsync();
        return View(model);
    }

    /// <summary>
    /// Cập nhật địa điểm làm việc (Chống IDOR: không cho sửa địa điểm của công ty khác).
    /// POST /nha-tuyen-dung/dia-diem/sua/{id}
    /// </summary>
    [HttpPost("sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkplaceInputModel model)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Provinces = await _geoService.GetProvincesAsync();
            return View(model);
        }

        var result = await _workplaceService.UpdateWorkplaceAsync(id, companyId.Value, model);
        if (!result.Success)
        {
            // Nếu thông báo lỗi là không tìm thấy -> 404
            if (result.Message != null && result.Message.Contains("Không tìm thấy"))
            {
                return NotFound(result.Message);
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Cập nhật địa điểm thất bại.");
            ViewBag.Provinces = await _geoService.GetProvincesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Xóa địa điểm làm việc (Chống IDOR: không cho xóa địa điểm của công ty khác, chặn khi có tin tuyển dụng).
    /// POST /nha-tuyen-dung/dia-diem/xoa/{id}
    /// </summary>
    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _workplaceService.DeleteWorkplaceAsync(id, companyId.Value);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("Không tìm thấy"))
            {
                return NotFound(result.Message);
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}

