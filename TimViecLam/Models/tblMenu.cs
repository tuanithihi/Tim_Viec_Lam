using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Menu điều hướng của sàn tuyển dụng.
/// </summary>
[Table("tblMenu")]
public class tblMenu
{
    [Key]
    public int MenuId { get; set; }

    [Required(ErrorMessage = "Tên menu không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên menu tối đa 100 ký tự.")]
    [Display(Name = "Tên menu")]
    public string MenuName { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Đường dẫn URL")]
    public string? Url { get; set; }

    [StringLength(50)]
    [Display(Name = "Biểu tượng (Icon CSS)")]
    public string? Icon { get; set; }

    [Display(Name = "Menu cha")]
    public int? ParentMenuId { get; set; }

    [Display(Name = "Thứ tự sắp xếp")]
    public int SortOrder { get; set; } = 0;

    [StringLength(30)]
    [Display(Name = "Vị trí")]
    public string? Position { get; set; } // "header" | "footer" | "sidebar"

    [Display(Name = "Mở trong tab mới")]
    public bool OpenNewTab { get; set; } = false;

    [Display(Name = "Trạng thái kích hoạt")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ── Self-referencing Navigation ─────────────────────────────────────
    [ForeignKey(nameof(ParentMenuId))]
    public virtual tblMenu? ParentMenu { get; set; }

    public virtual ICollection<tblMenu> ChildMenus { get; set; } = new List<tblMenu>();
}
