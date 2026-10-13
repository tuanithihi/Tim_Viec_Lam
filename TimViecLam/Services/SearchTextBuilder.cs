using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TimViecLam.Services;

/// <summary>
/// Trợ giúp chuẩn hóa chuỗi tìm kiếm không dấu, chữ thường dùng chung cho seed, tạo/sửa tin và truy vấn.
/// </summary>
public static class SearchTextBuilder
{
    private static readonly Regex MultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Chuẩn hóa chuỗi văn bản: loại bỏ dấu tiếng Việt, chuyển chữ thường, giữ lại ký tự chữ, số và một số ký tự lập trình như .NET, C#, C++, v.v.
    /// </summary>
    /// <param name="input">Chuỗi đầu vào cần chuẩn hóa.</param>
    /// <returns>Chuỗi đã chuẩn hóa, không ném lỗi khi input null hoặc rỗng.</returns>
    public static string Build(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Tách chuỗi Unicode (Form D) để tách các dấu thanh / dấu phụ
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                // Thay thế đ/Đ thành d/D
                if (c is 'đ' or 'Đ')
                {
                    sb.Append('d');
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        // Giữ lại chữ cái, số, khoảng trắng và các ký tự công nghệ phổ biến như '.', '#', '+'
        var cleaned = new StringBuilder(result.Length);
        foreach (var ch in result)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch) || ch is '.' or '#' or '+')
            {
                cleaned.Append(ch);
            }
            else
            {
                cleaned.Append(' ');
            }
        }

        // Thu gọn nhiều khoảng trắng liên tiếp về 1 khoảng trắng duy nhất
        var text = MultipleSpacesRegex.Replace(cleaned.ToString(), " ").Trim();
        if (!text.Any(char.IsLetterOrDigit))
        {
            return string.Empty;
        }

        return text;
    }

    /// <summary>
    /// Gộp nhiều thành phần (tiêu đề, công ty, kỹ năng, địa chỉ) thành một chuỗi tìm kiếm hoàn chỉnh.
    /// </summary>
    public static string BuildFromParts(params string?[] parts)
    {
        if (parts == null || parts.Length == 0)
        {
            return string.Empty;
        }

        var joined = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        return Build(joined);
    }
}
