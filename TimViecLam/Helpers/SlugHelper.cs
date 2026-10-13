using System.Text;
using System.Globalization;

namespace TimViecLam.Helpers;

/// <summary>
/// Trợ giúp tạo slug URL thân thiện từ chuỗi tiếng Việt.
/// </summary>
public static class SlugHelper
{
    /// <summary>
    /// Chuyển chuỗi tiếng Việt sang slug ASCII thân thiện URL.
    /// Ví dụ: "Lập Trình Viên .NET" → "lap-trinh-vien-net"
    /// </summary>
    /// <param name="text">Chuỗi đầu vào (có thể có dấu tiếng Việt).</param>
    /// <returns>Slug ASCII viết thường, ngăn cách bởi dấu gạch ngang.</returns>
    public static string Slugify(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Chuẩn hoá Unicode NFD để tách dấu
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) continue; // Bỏ dấu
            sb.Append(c);
        }

        var slug = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace("đ", "d")
            .Replace("Đ", "d");

        // Thay ký tự không phải chữ-số bằng dấu gạch ngang
        var result = new StringBuilder();
        bool lastWasDash = false;
        foreach (var c in slug)
        {
            if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash && result.Length > 0)
            {
                result.Append('-');
                lastWasDash = true;
            }
        }

        return result.ToString().Trim('-');
    }

    /// <summary>
    /// Tạo slug kết hợp chuỗi văn bản và ID số.
    /// Ví dụ: "Lập Trình Viên", 42 → "lap-trinh-vien-42"
    /// </summary>
    public static string Slugify(string? text, int id)
        => $"{Slugify(text)}-{id}";
}
