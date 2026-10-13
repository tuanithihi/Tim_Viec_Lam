using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimViecLam.Areas.Admin.Data;

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
            // Thay thế DataContext bằng InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("TimViecLamTest_" + Guid.NewGuid()));
        });
    }
}

/// <summary>
/// Đại diện môi trường SQLite in-memory tự động giải phóng tài nguyên.
/// </summary>
public sealed class SqliteTestDb : IDisposable
{
    public DataContext Context { get; }
    public SqliteConnection Connection { get; }

    public SqliteTestDb(DataContext context, SqliteConnection connection)
    {
        Context = context;
        Connection = connection;
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}

/// <summary>
/// Phương thức trợ giúp tạo DbContext độc lập cho unit test (hỗ trợ InMemory và SQLite).
/// </summary>
public static class TestDbHelper
{
    /// <summary>Tạo DataContext với InMemory database tên duy nhất.</summary>
    public static DataContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TimViecLamTest_" + Guid.NewGuid())
            .Options;
        return new DataContext(options);
    }

    /// <summary>Tạo DataContext với SQLite In-Memory database thực sự (hỗ trợ unique/foreign key constraints).</summary>
    public static SqliteTestDb CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options;

        var context = new DataContext(options);
        context.Database.EnsureCreated();
        return new SqliteTestDb(context, connection);
    }
}
