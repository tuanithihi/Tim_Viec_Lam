using Ganss.Xss;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ làm sạch HTML do người dùng nhập để chống tấn công XSS theo danh sách thẻ an toàn (Allow-list).
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Làm sạch chuỗi HTML, loại bỏ thẻ độc hại, thuộc tính event handler và các scheme nguy hiểm (javascript:, data:...).
    /// </summary>
    string Sanitize(string? html);
}

/// <summary>
/// Cài đặt dịch vụ làm sạch HTML sử dụng thư viện chuyên dụng HtmlSanitizer.
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();

        // Xóa trắng toàn bộ cấu hình mặc định để nạp chính xác danh sách an toàn
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedSchemes.Clear();

        // Danh sách thẻ văn bản và định dạng cho phép
        var safeTags = new[]
        {
            "p", "br", "b", "strong", "i", "em", "u", "s", "strike",
            "ul", "ol", "li",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "table", "thead", "tbody", "tr", "th", "td",
            "blockquote", "hr", "span", "div", "a"
        };
        foreach (var tag in safeTags)
        {
            _sanitizer.AllowedTags.Add(tag);
        }

        // Danh sách thuộc tính an toàn
        var safeAttributes = new[]
        {
            "href", "target", "rel", "title", "class", "style"
        };
        foreach (var attr in safeAttributes)
        {
            _sanitizer.AllowedAttributes.Add(attr);
        }

        // Thuộc tính CSS cơ bản (căn lề, màu sắc, phông chữ)
        var safeCss = new[]
        {
            "text-align", "font-weight", "font-style", "text-decoration", "color", "background-color"
        };
        foreach (var css in safeCss)
        {
            _sanitizer.AllowedCssProperties.Add(css);
        }

        // Scheme URL an toàn
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");
    }

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return _sanitizer.Sanitize(html);
    }
}

