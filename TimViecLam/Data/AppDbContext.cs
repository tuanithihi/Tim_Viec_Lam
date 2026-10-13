using Microsoft.EntityFrameworkCore;
using TimViecLam.Models.Entities;

namespace TimViecLam.Data;

/// <summary>
/// DbContext chính của ứng dụng TimViecLam.
/// Giai đoạn 0: chỉ khởi tạo khung, chưa có entity thực.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Khởi tạo context với options được inject từ DI.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Placeholder DbSet (sẽ bổ sung ở GĐ 1+) ──────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Cấu hình sẽ được thêm ở các giai đoạn sau
    }
}
