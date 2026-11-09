using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;

namespace TimViecLam.Areas.Admin.Controllers;

/// <summary>
/// Controller quản lý danh mục Kỹ năng chuyên môn dành riêng cho SuperAdmin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class SkillController : Controller
{
    private readonly ICategoryService _categoryService;

    public SkillController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /Admin/Skill
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _categoryService.GetPagedSkillsAsync(search, page, 10);
        ViewBag.Search = search ?? string.Empty;
        return View(result);
    }

    // POST: /Admin/Skill/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SkillInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.CreateSkillAsync(model);
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

    // POST: /Admin/Skill/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SkillInputModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            TempData["Error"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var result = await _categoryService.UpdateSkillAsync(model);
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

    // POST: /Admin/Skill/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _categoryService.ToggleSkillActiveAsync(id);
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

    // POST: /Admin/Skill/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteSkillAsync(id);
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

