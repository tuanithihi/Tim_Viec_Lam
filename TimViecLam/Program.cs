using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Services;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection");

// ── EF Core ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connection));

// ── MVC (AutoValidateAntiforgery toàn cục) ───────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// ── Cache (Dùng cho danh mục ít thay đổi) ────────────────────────────
builder.Services.AddMemoryCache();

// ── Session ──────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name        = ".TimViecLam.Session";
});

// ── HttpContext Accessor ──────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Health Check ──────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordService, PasswordService>();
// builder.Services.AddScoped<IJobService, JobService>();
// builder.Services.AddScoped<IEmployerService, EmployerService>();

var app = builder.Build();

// ── Tự động Seed dữ liệu Idempotent ───────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var passwordService = services.GetRequiredService<IPasswordService>();
        if (context.Database.CanConnect())
        {
            await DataSeeder.SeedAllAsync(context, passwordService);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Chưa thể kết nối CSDL để nạp dữ liệu seed khi khởi động.");
    }
}

// ── Pipeline ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Xử lý trang lỗi tùy biến (404, 403...) không lộ stack trace
app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ── Security Headers ──────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    // Ngăn MIME sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Ngăn clickjacking
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

    // Chính sách referrer
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Tắt các tính năng nhạy cảm không cần thiết
    context.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=(self), payment=()");

    // Content-Security-Policy: cho phép self + Bootstrap CDN + OSM tiles + Leaflet (unpkg)
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        "font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
        "img-src 'self' data: blob: https://*.tile.openstreetmap.org https://cdn.jsdelivr.net; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self'; " +
        "form-action 'self';");

    // KHÔNG dùng X-XSS-Protection (đã lỗi thời, có thể gây lỗ hổng)

    await next();
});

app.UseSession();       // ← phải đặt TRƯỚC UseAuthorization
app.UseAuthorization();

// ── Health Endpoint ───────────────────────────────────────────────────
app.MapHealthChecks("/health");

// ── Friendly Routes (khai báo TRƯỚC default route) ───────────────────

app.MapControllerRoute(
    name:    "viec_lam_detail",
    pattern: "viec-lam/{slug}-{id:int}",
    defaults: new { controller = "Job", action = "Detail" });

app.MapControllerRoute(
    name:    "viec_lam_list",
    pattern: "viec-lam",
    defaults: new { controller = "Job", action = "Index" });

app.MapControllerRoute(
    name:    "cong_ty_detail",
    pattern: "cong-ty/{slug}-{id:int}",
    defaults: new { controller = "Company", action = "Detail" });

app.MapControllerRoute(
    name:    "cong_ty_list",
    pattern: "cong-ty",
    defaults: new { controller = "Company", action = "Index" });

app.MapControllerRoute(
    name:    "lien_he",
    pattern: "lien-he",
    defaults: new { controller = "Home", action = "Contact" });

app.MapControllerRoute(
    name:    "dang_tin",
    pattern: "dang-tin",
    defaults: new { controller = "Employer", action = "PostJob" });

// ── Standard Routes ───────────────────────────────────────────────────
app.MapControllerRoute(
    name:    "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Để test tích hợp (Microsoft.AspNetCore.Mvc.Testing)
public partial class Program { }
