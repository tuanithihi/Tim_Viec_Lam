using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller quản lý danh mục Chế độ phúc lợi việc làm dành riêng cho SuperAdmin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class BenefitController : Controller
{
    private readonly ICategoryService _categoryService;

    public BenefitController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /Admin/Benefit
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _categoryService.GetPagedBenefitsAsync(search, page, 10);
        ViewBag.Search = search ?? string.Empty;
        return View(result);
    }

    // POST: /Admin/Benefit/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BenefitInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.CreateBenefitAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Admin/Benefit/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BenefitInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.UpdateBenefitAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Admin/Benefit/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _categoryService.ToggleBenefitActiveAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Admin/Benefit/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteBenefitAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}

