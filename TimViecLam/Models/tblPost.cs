using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimViecLam.Models;

/// <summary>
/// Thực thể Bài viết / Tin tức / Cẩm nang nghề nghiệp.
/// </summary>
[Table("tblPost")]
public class tblPost
{
    [Key]
    public int PostId { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống.")]
    [StringLength(300, ErrorMessage = "Tiêu đề tối đa 300 ký tự.")]
    [Display(Name = "Tiêu đề bài viết")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug không được để trống.")]
    [StringLength(350, ErrorMessage = "Slug tối đa 350 ký tự.")]
    [Display(Name = "Đường dẫn (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Tóm tắt ngắn")]
    public string? Summary { get; set; }

    [Display(Name = "Nội dung bài viết")]
    public string? Content { get; set; }

    [StringLength(300)]
    [Display(Name = "Ảnh đại diện")]
    public string? ThumbnailImage { get; set; }

    [StringLength(100)]
    [Display(Name = "Danh mục bài viết")]
    public string? Category { get; set; }

    [Display(Name = "Ghim lên đầu")]
    public bool IsPinned { get; set; } = false;

    [Display(Name = "Trạng thái đăng")]
    public bool IsPublished { get; set; } = false;

    [Display(Name = "Ngày đăng")]
    public DateTime? PublishedAt { get; set; }

    [Display(Name = "Lượt xem")]
    public int ViewCount { get; set; } = 0;

    [StringLength(300)]
    [Display(Name = "Tiêu đề SEO")]
    public string? MetaTitle { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả SEO")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdatedAt { get; set; }
}
