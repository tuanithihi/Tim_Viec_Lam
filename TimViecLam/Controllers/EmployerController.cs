using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Controllers;

/// <summary>
/// Controller quản lý hồ sơ công ty và trạng thái phê duyệt dành cho Nhà tuyển dụng.
/// </summary>
[Authorize(Policy = "EmployerOnly")]
[Route("nha-tuyen-dung")]
public class EmployerController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ICurrentUserService _currentUser;

    public EmployerController(ICompanyService companyService, ICurrentUserService currentUser)
    {
        _companyService = companyService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Trang tổng quan / Trạng thái phê duyệt của Nhà tuyển dụng.
    /// GET /nha-tuyen-dung/trang-thai
    /// </summary>
    [HttpGet("trang-thai")]
    public async Task<IActionResult> Status()
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var company = await _companyService.GetCompanyByIdAsync(companyId.Value);
        if (company == null)
        {
            return NotFound("Không tìm thấy thông tin công ty.");
        }

        return View(company);
    }

    /// <summary>
    /// Xem và chỉnh sửa hồ sơ công ty.
    /// GET /nha-tuyen-dung/ho-so
    /// </summary>
    [HttpGet("ho-so")]
    public async Task<IActionResult> Profile()
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var company = await _companyService.GetCompanyByIdAsync(companyId.Value);
        if (company == null)
        {
            return NotFound("Không tìm thấy thông tin công ty.");
        }

        var inputModel = new CompanyProfileInputModel
        {
            Name = company.Name,
            Website = company.Website,
            TaxCode = company.TaxCode,
            CompanySize = company.CompanySize,
            Description = company.Description,
            Phone = company.Phone,
            Email = company.Email,
            Address = company.Address
        };

        ViewBag.Company = company;
        return View(inputModel);
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ công ty (chống Over-posting: không nhận CompanyId hay Status từ form).
    /// POST /nha-tuyen-dung/ho-so
    /// </summary>
    [HttpPost("ho-so")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CompanyProfileInputModel model)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            var currentCompany = await _companyService.GetCompanyByIdAsync(companyId.Value);
            ViewBag.Company = currentCompany;
            return View(model);
        }

        var result = await _companyService.UpdateCompanyProfileAsync(companyId.Value, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Cập nhật hồ sơ thất bại.");
            var currentCompany = await _companyService.GetCompanyByIdAsync(companyId.Value);
            ViewBag.Company = currentCompany;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    /// <summary>
    /// Tải lên logo công ty.
    /// POST /nha-tuyen-dung/logo
    /// </summary>
    [HttpPost("logo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadLogo(IFormFile? logoFile)
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return Json(new { success = false, message = "Bạn chưa đăng nhập với tư cách Nhà tuyển dụng." });
        }

        var result = await _companyService.UploadLogoAsync(companyId.Value, logoFile);
        if (!result.Success)
        {
            return Json(new { success = false, message = result.Message });
        }

        return Json(new { success = true, message = result.Message, logoUrl = result.LogoPath });
    }

    /// <summary>
    /// Xóa logo công ty.
    /// POST /nha-tuyen-dung/xoa-logo
    /// </summary>
    [HttpPost("xoa-logo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogo()
    {
        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return Json(new { success = false, message = "Bạn chưa đăng nhập với tư cách Nhà tuyển dụng." });
        }

        var result = await _companyService.DeleteLogoAsync(companyId.Value);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Điều hướng nút Đăng tuyển (/dang-tin và /nha-tuyen-dung/dang-tin).
    /// Tự động xử lý trạng thái đăng nhập, phân quyền ứng viên và trạng thái phê duyệt công ty.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/dang-tin")]
    [HttpGet("dang-tin")]
    public async Task<IActionResult> PostJob()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Redirect("/dang-nhap?ReturnUrl=" + Uri.EscapeDataString("/dang-tin"));
        }

        if (User.IsInRole("Candidate"))
        {
            TempData["ErrorMessage"] = "Bạn đang đăng nhập bằng tài khoản Ứng viên. Vui lòng đăng ký hoặc sử dụng tài khoản Nhà tuyển dụng để đăng tin.";
            return RedirectToAction("RegisterEmployer", "Account");
        }

        int? companyId = _currentUser.CompanyId;
        if (!companyId.HasValue)
        {
            return RedirectToAction("Status");
        }

        var company = await _companyService.GetCompanyByIdAsync(companyId.Value);
        if (company == null || company.Status != "Approved")
        {
            return RedirectToAction("Status");
        }

        return RedirectToAction("Create", "EmployerJob");
    }

    /// <summary>
    /// Chuyển hướng tương thích ngược.
    /// </summary>
    [Authorize(Policy = "ApprovedEmployer")]
    [HttpGet("create-job-alias")]
    public IActionResult CreateJob()
    {
        return RedirectToAction("Create", "EmployerJob");
    }
}

