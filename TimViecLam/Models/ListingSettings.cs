namespace TimViecLam.Models;

/// <summary>
/// Cấu hình hiển thị và phê duyệt tin tuyển dụng từ appsettings.json.
/// </summary>
public class ListingSettings
{
    /// <summary>
    /// Tự động duyệt tin đăng khi nhà tuyển dụng gửi duyệt (bỏ qua kiểm duyệt thủ công).
    /// </summary>
    public bool AutoApprove { get; set; } = false;

    /// <summary>
    /// Số lượng ảnh tối đa đính kèm cho mỗi tin tuyển dụng.
    /// </summary>
    public int MaxImagesPerJob { get; set; } = 5;
}

