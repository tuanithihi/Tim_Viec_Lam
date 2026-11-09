using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint3A;

public class Sprint3A_LogoUploadTests
{
    private (FileUploadService service, string tempDir) CreateService()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "TimViecLam_UploadTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var service = new FileUploadService(mockEnv.Object, NullLogger<FileUploadService>.Instance);
        return (service, tempDir);
    }

    private IFormFile CreateMockFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "logoFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    // ── AC-S3A.3: File .exe đổi đuôi .png bị từ chối (Magic Bytes check) ─────────
    [Fact]
    public async Task AC_S3A_3_ExeFile_RenamedToPng_IsRejected()
    {
        var (service, tempDir) = CreateService();
        try
        {
            // Header MZ của Windows PE/EXE: 0x4D, 0x5A
            byte[] exeBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
            var file = CreateMockFile(exeBytes, "malicious.png", "image/png");

            var result = await service.UploadImageWithThumbnailAsync(file);
            Assert.False(result.IsValid);
            Assert.Contains("từ chối", result.ErrorMessage ?? "");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ── AC-S3A.3: File lớn hơn 2MB bị từ chối ────────────────────────────────────
    [Fact]
    public async Task AC_S3A_3_FileLargerThan2MB_IsRejected()
    {
        var (service, tempDir) = CreateService();
        try
        {
            // Tạo mảng byte > 2MB (2 * 1024 * 1024 + 10 bytes)
            byte[] largeBytes = new byte[2 * 1024 * 1024 + 10];
            // Gán đúng magic bytes của PNG để kiểm tra xem nó có bị chặn bởi kích thước không
            byte[] pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Array.Copy(pngHeader, largeBytes, pngHeader.Length);

            var file = CreateMockFile(largeBytes, "large_logo.png", "image/png");

            var result = await service.UploadImageWithThumbnailAsync(file);
            Assert.False(result.IsValid);
            Assert.Contains("vượt quá giới hạn", result.ErrorMessage ?? "");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ── AC-S3A.3: File SVG chứa script độc hại bị từ chối ───────────────────────
    [Theory]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert('xss')</script></svg>")]
    [InlineData("<svg onload=\"javascript:alert(1)\"><rect width=\"100\" height=\"100\"/></svg>")]
    [InlineData("<svg><a href=\"javascript:alert(1)\"><text>Click</text></a></svg>")]
    [InlineData("<!DOCTYPE svg [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><svg>&xxe;</svg>")]
    public async Task AC_S3A_3_SvgWithScriptOrDangerousPayload_IsRejected(string maliciousSvg)
    {
        var (service, tempDir) = CreateService();
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(maliciousSvg);
            var file = CreateMockFile(bytes, "vector.svg", "image/svg+xml");

            var result = await service.UploadImageWithThumbnailAsync(file);
            Assert.False(result.IsValid);
            Assert.Contains("không an toàn", result.ErrorMessage ?? "");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ── AC-S3A.3: File SVG an toàn hoặc ảnh hợp lệ được lưu và xóa dọn file cũ ───
    [Fact]
    public async Task AC_S3A_3_SafeSvg_IsAccepted_AndCleanedUponDelete()
    {
        var (service, tempDir) = CreateService();
        try
        {
            string safeSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\"><circle cx=\"50\" cy=\"50\" r=\"40\" fill=\"red\" /></svg>";
            byte[] bytes = Encoding.UTF8.GetBytes(safeSvg);
            var file = CreateMockFile(bytes, "safe_company_logo.svg", "image/svg+xml");

            var uploadResult = await service.UploadImageWithThumbnailAsync(file);
            Assert.True(uploadResult.IsValid);
            Assert.NotNull(uploadResult.RelativePath);

            string savedPhysicalPath = Path.Combine(tempDir, uploadResult.RelativePath.TrimStart('/'));
            Assert.True(File.Exists(savedPhysicalPath));

            // Thử dọn dẹp xóa file
            bool deleteResult = service.DeleteFile(uploadResult.RelativePath);
            Assert.True(deleteResult);
            Assert.False(File.Exists(savedPhysicalPath));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
