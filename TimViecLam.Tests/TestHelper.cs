using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimViecLam.Data;

namespace TimViecLam.Tests;

/// <summary>
/// Lớp trợ giúp dùng chung cho tất cả test giai đoạn.
/// Cung cấp WebApplicationFactory với DB InMemory.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Thay thế AppDbContext bằng InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TimViecLamTest_" + Guid.NewGuid()));
        });
    }
}

/// <summary>
/// Phương thức trợ giúp tạo DbContext InMemory độc lập cho unit test.
/// </summary>
public static class TestDbHelper
{
    /// <summary>Tạo AppDbContext với InMemory database tên duy nhất.</summary>
    public static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TimViecLamTest_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }
}
