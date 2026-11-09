using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt dịch vụ tải lên file an toàn, kiểm tra chữ ký định dạng thực tế và phòng chống tấn công tải mã độc.
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    private static readonly Dictionary<string, (string Mime, byte[][] Signatures)> AllowedRasterExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", ("image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } }) },
            { ".jpeg", ("image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } }) },
            { ".png", ("image/png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } }) },
            { ".gif", ("image/gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } }) },
            { ".webp", ("image/webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }) }
        };

    private static readonly Regex DangerousSvgPattern = new(
        @"(<\s*script|javascript\s*:|onload\s*=|onerror\s*=|onclick\s*=|onmouseover\s*=|onfocus\s*=|<!ENTITY)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<(bool IsValid, string? ErrorMessage, string? RelativePath, string? ThumbnailRelativePath)> UploadImageWithThumbnailAsync(
        IFormFile? file,
        string targetFolder = "uploads/logos",
        int thumbMaxWidth = 200,
        int thumbMaxHeight = 200,
        long maxFileSizeBytes = 2 * 1024 * 1024)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File tải lên không có dữ liệu.", null, null);
        }

        // 1. Kiểm tra kích thước file (tối đa 2MB mặc định)
        if (file.Length > maxFileSizeBytes)
        {
            long maxMb = maxFileSizeBytes / (1024 * 1024);
            return (false, $"Dung lượng file vượt quá giới hạn cho phép ({maxMb} MB).", null, null);
        }

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        bool isSvg = ext == ".svg";

        if (!isSvg && !AllowedRasterExtensions.ContainsKey(ext))
        {
            return (false, "Định dạng file không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .webp, .gif, .svg.", null, null);
        }

        try
        {
            using var stream = file.OpenReadStream();

            // 2. Kiểm tra chữ ký Magic bytes cho ảnh Raster hoặc kiểm tra mã độc cho SVG
            if (isSvg)
            {
                // Đọc toàn bộ nội dung SVG dưới dạng text để quét mã độc
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                string svgContent = await reader.ReadToEndAsync();

                if (!svgContent.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Nội dung file không phải là cấu trúc SVG hợp lệ.", null, null);
                }

                if (DangerousSvgPattern.IsMatch(svgContent))
                {
                    _logger.LogWarning("Phát hiện mã script độc hại trong file SVG tải lên: {FileName}", file.FileName);
                    return (false, "File SVG chứa mã kịch bản script không an toàn và bị từ chối.", null, null);
                }
            }
            else
            {
                var expectedInfo = AllowedRasterExtensions[ext];
                byte[] headerBytes = new byte[16];
                int bytesRead = await stream.ReadAsync(headerBytes.AsMemory(0, 16));

                // Chặn file thực thi PE / Windows EXE (bắt đầu bằng MZ: 0x4D, 0x5A)
                if (bytesRead >= 2 && headerBytes[0] == 0x4D && headerBytes[1] == 0x5A)
                {
                    _logger.LogWarning("Phát hiện file thực thi (.exe) đổi phần mở rộng: {FileName}", file.FileName);
                    return (false, "File thực thi bị từ chối.", null, null);
                }

                bool matchesSignature = false;
                foreach (var sig in expectedInfo.Signatures)
                {
                    if (bytesRead >= sig.Length && headerBytes.Take(sig.Length).SequenceEqual(sig))
                    {
                        matchesSignature = true;
                        break;
                    }
                }

                if (!matchesSignature)
                {
                    return (false, "Chữ ký nội dung file (Magic bytes) không hợp lệ hoặc file bị giả mạo phần mở rộng.", null, null);
                }
            }

            // 3. Đổi tên file ngẫu nhiên và lưu file gốc
            string guidPrefix = Guid.NewGuid().ToString("N");
            string uniqueFileName = string.Concat(guidPrefix, ext);
            string uploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), targetFolder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fullPath = Path.Combine(uploadPath, uniqueFileName);
            stream.Position = 0;

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            string relativeUrl = "/" + targetFolder.Trim('/') + "/" + uniqueFileName;
            string? thumbRelativeUrl = null;

            // 4. Tạo thumbnail bằng ImageSharp nếu là ảnh Raster
            if (!isSvg && thumbMaxWidth > 0 && thumbMaxHeight > 0)
            {
                try
                {
                    string thumbFileName = string.Concat(guidPrefix, "_thumb", ext);
                    string thumbFullPath = Path.Combine(uploadPath, thumbFileName);

                    stream.Position = 0;
                    using var image = await Image.LoadAsync(stream);
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(thumbMaxWidth, thumbMaxHeight)
                    }));

                    await image.SaveAsync(thumbFullPath);
                    thumbRelativeUrl = "/" + targetFolder.Trim('/') + "/" + thumbFileName;
                }
                catch (Exception thumbEx)
                {
                    _logger.LogWarning(thumbEx, "Không thể tạo thumbnail cho file: {FileName}. Sử dụng ảnh gốc làm fallback.", uniqueFileName);
                    thumbRelativeUrl = relativeUrl;
                }
            }
            else
            {
                // Với SVG hoặc khi không resize, thumbnail trỏ về file chính
                thumbRelativeUrl = relativeUrl;
            }

            return (true, null, relativeUrl, thumbRelativeUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình xử lý tải lên file.");
            return (false, "Đã xảy ra sự cố trong quá trình lưu file.", null, null);
        }
    }

    public bool DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        try
        {
            string cleanPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), cleanPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                // Dọn dẹp cả file thumbnail đi kèm nếu có
                string ext = Path.GetExtension(fullPath);
                string withoutExt = fullPath[..^ext.Length];
                if (!withoutExt.EndsWith("_thumb"))
                {
                    string possibleThumb = withoutExt + "_thumb" + ext;
                    if (File.Exists(possibleThumb))
                    {
                        File.Delete(possibleThumb);
                    }
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể xóa file: {Path}", relativePath);
        }

        return false;
    }
}
