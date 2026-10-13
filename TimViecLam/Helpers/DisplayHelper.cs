namespace TimViecLam.Helpers;

/// <summary>
/// Trợ giúp hiển thị dữ liệu cho View (khung, sẽ bổ sung ở GĐ sau).
/// </summary>
public static class DisplayHelper
{
    /// <summary>Định dạng mức lương theo kiểu tiền tệ Việt Nam.</summary>
    /// <param name="amount">Số tiền (đơn vị: VND).</param>
    /// <returns>Chuỗi định dạng, ví dụ: "15.000.000 ₫"</returns>
    public static string FormatSalary(decimal? amount)
    {
        if (!amount.HasValue || amount <= 0) return "Thỏa thuận";
        return $"{amount.Value:N0} ₫";
    }

    /// <summary>Rút gọn chuỗi nếu vượt quá độ dài tối đa.</summary>
    /// <param name="text">Chuỗi đầu vào.</param>
    /// <param name="maxLength">Độ dài tối đa (mặc định 150).</param>
    public static string Truncate(string? text, int maxLength = 150)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        if (text.Length <= maxLength) return text;
        return text[..maxLength].TrimEnd() + "…";
    }

    /// <summary>Trả về URL avatar mặc định nếu avatar là null/rỗng.</summary>
    public static string AvatarUrl(string? avatar, string defaultPath = "/images/avatar-default.svg")
        => string.IsNullOrWhiteSpace(avatar) ? defaultPath : avatar;

    /// <summary>Hiển thị khoảng thời gian đăng tin dạng "x ngày trước".</summary>
    public static string TimeAgo(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return string.Empty;
        var span = DateTime.UtcNow - dateTime.Value.ToUniversalTime();
        if (span.TotalMinutes < 1) return "vừa xong";
        if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} phút trước";
        if (span.TotalDays < 1) return $"{(int)span.TotalHours} giờ trước";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays} ngày trước";
        if (span.TotalDays < 365) return $"{(int)(span.TotalDays / 30)} tháng trước";
        return $"{(int)(span.TotalDays / 365)} năm trước";
    }
}
