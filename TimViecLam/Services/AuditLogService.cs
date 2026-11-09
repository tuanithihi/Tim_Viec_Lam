using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ ghi nhận nhật ký kiểm toán (Audit Log) cho các hành động quan trọng trong hệ thống.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Ghi nhận một sự kiện kiểm toán mới vào CSDL.
    /// </summary>
    Task LogAsync(int? userId, string action, string entityName, string? entityId, string? details, string? ipAddress = null);
}

/// <summary>
/// Cài đặt dịch vụ ghi nhật ký kiểm toán lưu vào bảng tblAuditLog.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly DataContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(DataContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(int? userId, string action, string entityName, string? entityId, string? details, string? ipAddress = null)
    {
        try
        {
            var log = new tblAuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Bắt ngoại lệ để tránh làm gián đoạn luồng nghiệp vụ chính nếu việc ghi log gặp sự cố tạm thời
            _logger.LogError(ex, "Lỗi ghi nhận nhật ký kiểm toán cho hành động {Action} trên thực thể {EntityName} id {EntityId}",
                action, entityName, entityId);
        }
    }
}

