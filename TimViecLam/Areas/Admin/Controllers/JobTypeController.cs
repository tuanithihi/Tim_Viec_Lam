using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller quản lý danh mục Hình thức làm việc dành riêng cho SuperAdmin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class JobTypeController : Controller
{
    private readonly ICategoryService _categoryService;

    public JobTypeController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /Admin/JobType
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _categoryService.GetPagedJobTypesAsync(search, page, 10);
        ViewBag.Search = search ?? string.Empty;
        return View(result);
    }

    // POST: /Admin/JobType/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobTypeInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.CreateJobTypeAsync(model);
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

    // POST: /Admin/JobType/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(JobTypeInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.UpdateJobTypeAsync(model);
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

    // POST: /Admin/JobType/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _categoryService.ToggleJobTypeActiveAsync(id);
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

    // POST: /Admin/JobType/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteJobTypeAsync(id);
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

