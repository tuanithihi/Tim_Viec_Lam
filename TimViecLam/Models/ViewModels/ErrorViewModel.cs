namespace TimViecLam.Models.ViewModels;

/// <summary>ViewModel cho trang lỗi 500.</summary>
public class ErrorViewModel
{
    /// <summary>Request ID để truy vết lỗi.</summary>
    public string? RequestId { get; set; }

    /// <summary>Hiển thị RequestId nếu không rỗng.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
