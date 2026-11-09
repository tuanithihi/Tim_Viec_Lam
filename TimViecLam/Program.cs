using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
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

// ── Cookie Authentication ─────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".TimViecLam.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/dang-nhap";
        options.LogoutPath = "/dang-xuat";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
                var userIdStr = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int userId))
                {
                    var user = await db.Users.Include(u => u.Company).AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null || !user.IsActive)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        return;
                    }

                    // Employer bị Suspended giữa phiên -> request kế tiếp bị chặn (AC-S1.5)
                    if (user.Role == "Employer" && user.Company != null)
                    {
                        if (user.Company.Status == "Suspended")
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }

                        // Tự động đồng bộ claim CompanyStatus khi trạng thái công ty được SuperAdmin thay đổi
                        var currentStatusClaim = context.Principal?.FindFirst("CompanyStatus")?.Value;
                        if (currentStatusClaim != user.Company.Status)
                        {
                            if (context.Principal?.Identity is ClaimsIdentity identity)
                            {
                                var oldClaim = identity.FindFirst("CompanyStatus");
                                if (oldClaim != null) identity.RemoveClaim(oldClaim);
                                identity.AddClaim(new Claim("CompanyStatus", user.Company.Status));
                                context.ShouldRenew = true;
                            }
                        }
                    }
                }
                else
                {
                    context.RejectPrincipal();
                }
            }
        };
    });

// ── Phân quyền (Authorization Policies) ───────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CandidateOnly", p => p.RequireRole("Candidate"));
    options.AddPolicy("EmployerOnly", p => p.RequireRole("Employer"));
    options.AddPolicy("SuperAdminOnly", p => p.RequireRole("SuperAdmin"));
    options.AddPolicy("ApprovedEmployer", p => p.RequireRole("Employer").RequireAssertion(ctx =>
    {
        return ctx.User.FindFirst("CompanyStatus")?.Value == "Approved";
    }));
});

// ── Rate Limiter ──────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
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

// ── Cấu hình ListingSettings ──────────────────────────────────────────
builder.Services.Configure<ListingSettings>(builder.Configuration.GetSection("ListingSettings"));

// ── Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGeoService, GeoService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddHttpClient<INominatimGeocodeService, NominatimGeocodeService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IWorkplaceService, WorkplaceService>();
builder.Services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IJobManagementService, JobManagementService>();
builder.Services.AddScoped<IJobModerationService, JobModerationService>();

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

// Xử lý trang lỗi 404 không lộ stack trace
app.UseStatusCodePages(context =>
{
    if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound && !context.HttpContext.Response.HasStarted)
    {
        context.HttpContext.Response.Redirect("/Home/NotFound");
    }
    return Task.CompletedTask;
});

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

    // Content-Security-Policy: cho phép self + Bootstrap CDN + OSM/Carto/Esri map tiles + Leaflet
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        "font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
        "img-src 'self' data: blob: https://*.tile.openstreetmap.org https://tile.openstreetmap.de https://*.tile.openstreetmap.de https://*.tile.openstreetmap.fr https://*.basemaps.cartocdn.com https://server.arcgisonline.com https://cdn.jsdelivr.net; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self'; " +
        "form-action 'self';");

    // KHÔNG dùng X-XSS-Protection (đã lỗi thời, có thể gây lỗ hổng)

    await next();
});

app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ── Health Endpoint ───────────────────────────────────────────────────
app.MapHealthChecks("/health");

// ── Friendly Routes Cho Tài Khoản (khai báo TRƯỚC default route) ──────
app.MapControllerRoute(
    name:    "dang_nhap",
    pattern: "dang-nhap",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name:    "dang_ky",
    pattern: "dang-ky",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name:    "dang_ky_nha_tuyen_dung",
    pattern: "dang-ky-nha-tuyen-dung",
    defaults: new { controller = "Account", action = "RegisterEmployer" });

app.MapControllerRoute(
    name:    "dang_xuat",
    pattern: "dang-xuat",
    defaults: new { controller = "Account", action = "Logout" });

app.MapControllerRoute(
    name:    "ho_so",
    pattern: "ho-so",
    defaults: new { controller = "Account", action = "Profile" });

app.MapControllerRoute(
    name:    "doi_mat_khau",
    pattern: "doi-mat-khau",
    defaults: new { controller = "Account", action = "ChangePassword" });

app.MapControllerRoute(
    name:    "quen_mat_khau",
    pattern: "quen-mat-khau",
    defaults: new { controller = "Account", action = "ForgotPassword" });

app.MapControllerRoute(
    name:    "dat_lai_mat_khau",
    pattern: "dat-lai-mat-khau",
    defaults: new { controller = "Account", action = "ResetPassword" });

// ── Friendly Routes Cho Việc Làm & Doanh Nghiệp ────────────────────────
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

// ── Area Routes ───────────────────────────────────────────────────────
app.MapControllerRoute(
    name:    "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// ── Default Route ─────────────────────────────────────────────────────
app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

// Để test tích hợp (Microsoft.AspNetCore.Mvc.Testing)
public partial class Program { }
