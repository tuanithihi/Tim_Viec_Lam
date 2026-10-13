using Microsoft.AspNetCore.Mvc;

namespace TimViecLam.Components;

/// <summary>
/// ViewComponent hiển thị menu điều hướng chính của site.
/// Giai đoạn 0: render tĩnh, không truy vấn DB.
/// </summary>
[ViewComponent(Name = "MenuView")]
public class MenuViewComponent : ViewComponent
{
    /// <summary>Render menu điều hướng.</summary>
    public IViewComponentResult Invoke()
    {
        return View("Default");
    }
}
