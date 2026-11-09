using Microsoft.AspNetCore.Http;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện xử lý tải lên, kiểm tra bảo mật (kích thước, MIME, magic bytes, kịch bản độc hại) và dọn dẹp file.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Tải lên ảnh, kiểm tra magic bytes, chống file thực thi giả mạo, kiểm tra script độc hại đối với SVG,
    /// sinh thumbnail bằng ImageSharp và lưu tên file ngẫu nhiên.
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage, string? RelativePath, string? ThumbnailRelativePath)> UploadImageWithThumbnailAsync(
        IFormFile? file,
        string targetFolder = "uploads/logos",
        int thumbMaxWidth = 200,
        int thumbMaxHeight = 200,
        long maxFileSizeBytes = 2 * 1024 * 1024);

    /// <summary>
    /// Xóa file và thumbnail liên quan khỏi hệ thống đĩa.
    /// </summary>
    bool DeleteFile(string? relativePath);
}

