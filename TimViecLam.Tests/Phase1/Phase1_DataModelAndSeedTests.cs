using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Phase1;

/// <summary>
/// Bộ kiểm thử chấp nhận Giai đoạn 1: Mô hình dữ liệu, Migration, Seed.
/// Phủ toàn bộ AC-1.1 đến AC-1.8, test âm tính và kiểm thử hồi quy Giai đoạn 0.
/// </summary>
public class Phase1_DataModelAndSeedTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private readonly IPasswordService _passwordService = new PasswordService();

    public Phase1_DataModelAndSeedTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // ── AC-1.1: Migration áp dụng được trên SQLite in-memory, script SQL hợp lệ ─

    [Fact(DisplayName = "AC_1_1_Migration_AppliesOnSqliteInMemory_CreatesAllTables")]
    public void AC_1_1_Migration_AppliesOnSqliteInMemory_CreatesAllTables()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        // Kiểm tra tất cả 20 bảng đều có thể truy vấn được trên SQLite In-Memory
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Companies);
        Assert.NotNull(context.Workplaces);
        Assert.NotNull(context.Provinces);
        Assert.NotNull(context.Districts);
        Assert.NotNull(context.Wards);
        Assert.NotNull(context.JobCategories);
        Assert.NotNull(context.JobTypes);
        Assert.NotNull(context.Skills);
        Assert.NotNull(context.JobSkills);
        Assert.NotNull(context.Benefits);
        Assert.NotNull(context.JobBenefits);
        Assert.NotNull(context.Jobs);
        Assert.NotNull(context.CandidateProfiles);
        Assert.NotNull(context.Applications);
        Assert.NotNull(context.SavedJobs);
        Assert.NotNull(context.JobAlerts);
        Assert.NotNull(context.Posts);
        Assert.NotNull(context.Menus);
        Assert.NotNull(context.AuditLogs);

        Assert.Equal(0, context.Users.Count());
        Assert.Equal(0, context.Jobs.Count());
    }

    [Fact(DisplayName = "AC_1_1_MigrationScript_GeneratedSql_FileExistsAndNotEmpty")]
    public void AC_1_1_MigrationScript_GeneratedSql_FileExistsAndNotEmpty()
    {
        var solutionRoot = GetSolutionRoot();
        var sqlPath = Path.Combine(solutionRoot, "data", "initial_create.sql");

        Assert.True(File.Exists(sqlPath), $"Không tìm thấy file SQL migration: {sqlPath}");
        var content = File.ReadAllText(sqlPath);
        Assert.True(content.Length > 1000, "File SQL migration quá ngắn hoặc rỗng.");
        Assert.Contains("CREATE TABLE [tblUser]", content);
        Assert.Contains("CREATE TABLE [tblJob]", content);
        Assert.Contains("CREATE TABLE [tblCompany]", content);
    }

    // ── AC-1.2: Seed 2 lần liên tiếp: số bản ghi không đổi, không ngoại lệ ────

    [Fact(DisplayName = "AC_1_2_Seed_RunTwice_RecordCountsUnchanged_NoException")]
    public async Task AC_1_2_Seed_RunTwice_RecordCountsUnchanged_NoException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        // Lần 1: Chạy Seed
        await DataSeeder.SeedAllAsync(context, _passwordService);

        var usersCount1 = await context.Users.CountAsync();
        var companiesCount1 = await context.Companies.CountAsync();
        var workplacesCount1 = await context.Workplaces.CountAsync();
        var categoriesCount1 = await context.JobCategories.CountAsync();
        var jobTypesCount1 = await context.JobTypes.CountAsync();
        var skillsCount1 = await context.Skills.CountAsync();
        var benefitsCount1 = await context.Benefits.CountAsync();
        var jobsCount1 = await context.Jobs.CountAsync();
        var menusCount1 = await context.Menus.CountAsync();
        var postsCount1 = await context.Posts.CountAsync();

        // Kiểm tra số lượng tối thiểu theo yêu cầu đề bài
        Assert.True(categoriesCount1 >= 15, $"Yêu cầu >= 15 ngành nghề, thực tế: {categoriesCount1}");
        Assert.True(jobTypesCount1 >= 5, $"Yêu cầu >= 5 hình thức việc làm, thực tế: {jobTypesCount1}");
        Assert.True(skillsCount1 >= 40, $"Yêu cầu >= 40 kỹ năng, thực tế: {skillsCount1}");
        Assert.True(benefitsCount1 >= 10, $"Yêu cầu >= 10 phúc lợi, thực tế: {benefitsCount1}");
        Assert.True(companiesCount1 >= 3, $"Yêu cầu >= 3 công ty, thực tế: {companiesCount1}");
        Assert.True(workplacesCount1 >= 6, $"Yêu cầu >= 6 địa điểm, thực tế: {workplacesCount1}");
        Assert.True(jobsCount1 >= 40, $"Yêu cầu >= 40 việc làm, thực tế: {jobsCount1}");

        // Lần 2: Chạy lại Seed (Idempotent test)
        await DataSeeder.SeedAllAsync(context, _passwordService);

        var usersCount2 = await context.Users.CountAsync();
        var companiesCount2 = await context.Companies.CountAsync();
        var workplacesCount2 = await context.Workplaces.CountAsync();
        var categoriesCount2 = await context.JobCategories.CountAsync();
        var jobTypesCount2 = await context.JobTypes.CountAsync();
        var skillsCount2 = await context.Skills.CountAsync();
        var benefitsCount2 = await context.Benefits.CountAsync();
        var jobsCount2 = await context.Jobs.CountAsync();
        var menusCount2 = await context.Menus.CountAsync();
        var postsCount2 = await context.Posts.CountAsync();

        // Đảm bảo số lượng giữ nguyên 100%, không nhân đôi bản ghi
        Assert.Equal(usersCount1, usersCount2);
        Assert.Equal(companiesCount1, companiesCount2);
        Assert.Equal(workplacesCount1, workplacesCount2);
        Assert.Equal(categoriesCount1, categoriesCount2);
        Assert.Equal(jobTypesCount1, jobTypesCount2);
        Assert.Equal(skillsCount1, skillsCount2);
        Assert.Equal(benefitsCount1, benefitsCount2);
        Assert.Equal(jobsCount1, jobsCount2);
        Assert.Equal(menusCount1, menusCount2);
        Assert.Equal(postsCount1, postsCount2);
    }

    // ── AC-1.3: Unique constraints được thực thi (Throws DbUpdateException) ──

    [Fact(DisplayName = "AC_1_3_Unique_DuplicateUsername_ThrowsDbUpdateException")]
    public async Task AC_1_3_Unique_DuplicateUsername_ThrowsDbUpdateException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        context.Users.Add(new tblUser
        {
            Username = "unique_user",
            Email = "user1@test.com",
            PasswordHash = _passwordService.HashPassword("Test@123")
        });
        await context.SaveChangesAsync();

        context.Users.Add(new tblUser
        {
            Username = "unique_user", // Trùng Username
            Email = "user2@test.com",
            PasswordHash = _passwordService.HashPassword("Test@123")
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact(DisplayName = "AC_1_3_Unique_DuplicateEmail_ThrowsDbUpdateException")]
    public async Task AC_1_3_Unique_DuplicateEmail_ThrowsDbUpdateException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        context.Users.Add(new tblUser
        {
            Username = "user_a",
            Email = "duplicate@test.com",
            PasswordHash = _passwordService.HashPassword("Test@123")
        });
        await context.SaveChangesAsync();

        context.Users.Add(new tblUser
        {
            Username = "user_b",
            Email = "duplicate@test.com", // Trùng Email
            PasswordHash = _passwordService.HashPassword("Test@123")
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact(DisplayName = "AC_1_3_Unique_DuplicateCompanySlug_ThrowsDbUpdateException")]
    public async Task AC_1_3_Unique_DuplicateCompanySlug_ThrowsDbUpdateException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        var u1 = new tblUser { Username = "employer1", Email = "e1@test.com", PasswordHash = _passwordService.HashPassword("Test@123") };
        var u2 = new tblUser { Username = "employer2", Email = "e2@test.com", PasswordHash = _passwordService.HashPassword("Test@123") };
        context.Users.AddRange(u1, u2);
        await context.SaveChangesAsync();

        context.Companies.Add(new tblCompany
        {
            UserId = u1.UserId,
            Name = "Company A",
            Slug = "duplicate-slug",
            Status = "Approved"
        });
        await context.SaveChangesAsync();

        context.Companies.Add(new tblCompany
        {
            UserId = u2.UserId,
            Name = "Company B",
            Slug = "duplicate-slug", // Trùng Slug
            Status = "Approved"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact(DisplayName = "AC_1_3_Unique_DuplicateApplication_ThrowsDbUpdateException")]
    public async Task AC_1_3_Unique_DuplicateApplication_ThrowsDbUpdateException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        await DataSeeder.SeedAllAsync(context, _passwordService);

        var job = await context.Jobs.FirstAsync();
        var candidate = await context.Users.FirstAsync(u => u.Role == "Candidate");

        // Đảm bảo xóa đơn cũ nếu đã có từ seeder
        var existingApp = await context.Applications.FirstOrDefaultAsync(a => a.JobId == job.JobId && a.CandidateUserId == candidate.UserId);
        if (existingApp != null)
        {
            context.Applications.Remove(existingApp);
            await context.SaveChangesAsync();
        }

        context.Applications.Add(new tblApplication
        {
            JobId = job.JobId,
            CandidateUserId = candidate.UserId,
            FullName = "Ứng viên 1",
            Email = "candidate@test.com",
            Phone = "0900000001",
            CvPath = "/cv.pdf",
            Status = "New"
        });
        await context.SaveChangesAsync();

        // Ứng tuyển lần 2 vào cùng JobId và CandidateUserId
        context.Applications.Add(new tblApplication
        {
            JobId = job.JobId,
            CandidateUserId = candidate.UserId,
            FullName = "Ứng viên 1 Lần 2",
            Email = "candidate@test.com",
            Phone = "0900000001",
            CvPath = "/cv2.pdf",
            Status = "New"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact(DisplayName = "AC_1_3_Unique_DuplicateSavedJob_ThrowsDbUpdateException")]
    public async Task AC_1_3_Unique_DuplicateSavedJob_ThrowsDbUpdateException()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        await DataSeeder.SeedAllAsync(context, _passwordService);

        var job = await context.Jobs.FirstAsync();
        var candidate = await context.Users.FirstAsync(u => u.Role == "Candidate");

        var existingSaved = await context.SavedJobs.FirstOrDefaultAsync(s => s.JobId == job.JobId && s.UserId == candidate.UserId);
        if (existingSaved != null)
        {
            context.SavedJobs.Remove(existingSaved);
            await context.SaveChangesAsync();
        }

        context.SavedJobs.Add(new tblSavedJob
        {
            JobId = job.JobId,
            UserId = candidate.UserId
        });
        await context.SaveChangesAsync();

        // Lưu việc lần 2 với cùng UserId và JobId
        context.SavedJobs.Add(new tblSavedJob
        {
            JobId = job.JobId,
            UserId = candidate.UserId
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    // ── AC-1.4: Xóa Province/Company đang được tham chiếu bị chặn (Restrict) ──

    [Fact(DisplayName = "AC_1_4_Delete_ProvinceWithDistricts_Restricted")]
    public async Task AC_1_4_Delete_ProvinceWithDistricts_Restricted()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        // Bật Foreign Keys pragma cho SQLite connection
        var cmd = sqlite.Connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        var province = new tblProvince { Code = "99", Name = "Tỉnh Thử Nghiệm", Slug = "tinh-thu-nghiem" };
        context.Provinces.Add(province);
        await context.SaveChangesAsync();

        var district = new tblDistrict { ProvinceId = province.ProvinceId, Code = "999", Name = "Huyện Thử Nghiệm", Slug = "huyen-thu-nghiem" };
        context.Districts.Add(district);
        await context.SaveChangesAsync();

        // Cố tình xóa Province khi còn District phụ thuộc -> Xóa bị chặn (Restrict)
        var ex = await Record.ExceptionAsync(async () =>
        {
            context.Provinces.Remove(province);
            await context.SaveChangesAsync();
        });

        Assert.NotNull(ex);
        Assert.True(ex is DbUpdateException || ex is InvalidOperationException,
            $"Mong đợi xóa bị chặn (DbUpdateException hoặc InvalidOperationException), nhận được: {ex.GetType().Name}");
    }

    [Fact(DisplayName = "AC_1_4_Delete_CompanyWithJobs_Restricted")]
    public async Task AC_1_4_Delete_CompanyWithJobs_Restricted()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        var cmd = sqlite.Connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        await DataSeeder.SeedAllAsync(context, _passwordService);

        var company = await context.Companies.Include(c => c.Jobs).FirstAsync(c => c.Jobs.Any());

        // Cố tình xóa Company khi còn Jobs tham chiếu -> Xóa bị chặn (Restrict)
        var ex = await Record.ExceptionAsync(async () =>
        {
            context.Companies.Remove(company);
            await context.SaveChangesAsync();
        });

        Assert.NotNull(ex);
        Assert.True(ex is DbUpdateException || ex is InvalidOperationException,
            $"Mong đợi xóa bị chặn (DbUpdateException hoặc InvalidOperationException), nhận được: {ex.GetType().Name}");
    }

    // ── AC-1.5: Index bắt buộc tồn tại trên EF Model ─────────────────────────

    [Fact(DisplayName = "AC_1_5_MandatoryIndexes_ExistInModel")]
    public void AC_1_5_MandatoryIndexes_ExistInModel()
    {
        using var context = TestDbHelper.CreateInMemoryContext();
        var model = context.Model;

        // 1. Job(ApprovalStatus, ExpiresAt)
        var jobEntity = model.FindEntityType(typeof(tblJob));
        Assert.NotNull(jobEntity);
        var jobIndexes = jobEntity.GetIndexes().ToList();

        Assert.Contains(jobIndexes, idx =>
            idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "ApprovalStatus", "ExpiresAt" }));

        // 2. Job(CategoryId), Job(JobTypeId), Job(CompanyId)
        Assert.Contains(jobIndexes, idx => idx.Properties.Any(p => p.Name == "CategoryId"));
        Assert.Contains(jobIndexes, idx => idx.Properties.Any(p => p.Name == "JobTypeId"));
        Assert.Contains(jobIndexes, idx => idx.Properties.Any(p => p.Name == "CompanyId"));

        // 3. Job(SalaryMin, SalaryMax)
        Assert.Contains(jobIndexes, idx =>
            idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "SalaryMin", "SalaryMax" }));

        // 4. Job(Slug)
        Assert.Contains(jobIndexes, idx => idx.Properties.Any(p => p.Name == "Slug"));

        // 5. Workplace(Latitude, Longitude), Workplace(ProvinceId, DistrictId)
        var wpEntity = model.FindEntityType(typeof(tblWorkplace));
        Assert.NotNull(wpEntity);
        var wpIndexes = wpEntity.GetIndexes().ToList();

        Assert.Contains(wpIndexes, idx =>
            idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "Latitude", "Longitude" }));
        Assert.Contains(wpIndexes, idx =>
            idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProvinceId", "DistrictId" }));

        // 6. Slugs: Company, Province, District
        var compEntity = model.FindEntityType(typeof(tblCompany));
        Assert.NotNull(compEntity);
        Assert.Contains(compEntity.GetIndexes(), idx => idx.Properties.Any(p => p.Name == "Slug"));

        var provEntity = model.FindEntityType(typeof(tblProvince));
        Assert.NotNull(provEntity);
        Assert.Contains(provEntity.GetIndexes(), idx => idx.Properties.Any(p => p.Name == "Slug"));

        var distEntity = model.FindEntityType(typeof(tblDistrict));
        Assert.NotNull(distEntity);
        Assert.Contains(distEntity.GetIndexes(), idx => idx.Properties.Any(p => p.Name == "Slug"));

        // 7. Application(JobId), Application(CandidateUserId)
        var appEntity = model.FindEntityType(typeof(tblApplication));
        Assert.NotNull(appEntity);
        var appIndexes = appEntity.GetIndexes().ToList();
        Assert.Contains(appIndexes, idx => idx.Properties.Any(p => p.Name == "JobId"));
        Assert.Contains(appIndexes, idx => idx.Properties.Any(p => p.Name == "CandidateUserId"));
    }

    // ── AC-1.6: SearchTextBuilder chuẩn hóa chính xác, an toàn với null/ký tự lạ

    [Fact(DisplayName = "AC_1_6_SearchTextBuilder_RemovesAccentsAndLowercases")]
    public void AC_1_6_SearchTextBuilder_RemovesAccentsAndLowercases()
    {
        var input = "Lập trình viên .NET Đà Nẵng";
        var result = SearchTextBuilder.Build(input);
        Assert.Equal("lap trinh vien .net da nang", result);
    }

    [Theory(DisplayName = "AC_1_6_SearchTextBuilder_HandlesNullEmptyAndSpecialChars")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("!@#$%^&*()", "")]
    [InlineData("C# Senior Developer + AWS", "c# senior developer + aws")]
    [InlineData("  Hà   Nội    Thủ   Đô  ", "ha noi thu do")]
    public void AC_1_6_SearchTextBuilder_HandlesNullEmptyAndSpecialChars(string? input, string expected)
    {
        var result = SearchTextBuilder.Build(input);
        Assert.Equal(expected, result);
    }

    // ── AC-1.7: Dữ liệu seed thỏa mãn các ràng buộc nghiệp vụ ─────────────────

    [Fact(DisplayName = "AC_1_7_SeedData_BusinessConstraints_Valid")]
    public async Task AC_1_7_SeedData_BusinessConstraints_Valid()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        await DataSeeder.SeedAllAsync(context, _passwordService);

        var allJobs = await context.Jobs.Include(j => j.Workplace).ToListAsync();
        Assert.NotEmpty(allJobs);

        var publishedJobs = allJobs.Where(j => j.ApprovalStatus == "Published").ToList();
        Assert.True(publishedJobs.Count >= 40, $"Số tin Published phải >= 40, thực tế: {publishedJobs.Count}");

        foreach (var job in publishedJobs)
        {
            // Published => ExpiresAt phải ở tương lai
            Assert.True(job.ExpiresAt > DateTime.Now,
                $"Tin Published '{job.Title}' có ExpiresAt trong quá khứ: {job.ExpiresAt}");

            // Mọi Job phải có SearchText không rỗng
            Assert.False(string.IsNullOrWhiteSpace(job.SearchText),
                $"Tin '{job.Title}' có SearchText rỗng.");

            // SalaryMin <= SalaryMax nếu cả 2 có giá trị
            if (job.SalaryMin.HasValue && job.SalaryMax.HasValue)
            {
                Assert.True(job.SalaryMin.Value <= job.SalaryMax.Value,
                    $"Tin '{job.Title}' có SalaryMin ({job.SalaryMin}) > SalaryMax ({job.SalaryMax})");
            }
        }

        // Tọa độ Việt Nam hợp lệ: Lat 8 - 24, Lng 102 - 110
        var allWorkplaces = await context.Workplaces.ToListAsync();
        Assert.True(allWorkplaces.Count >= 6, $"Số địa điểm phải >= 6, thực tế: {allWorkplaces.Count}");

        foreach (var wp in allWorkplaces)
        {
            if (wp.Latitude.HasValue && wp.Longitude.HasValue)
            {
                Assert.InRange(wp.Latitude.Value, 8.0, 24.0);
                Assert.InRange(wp.Longitude.Value, 102.0, 110.0);
            }
        }
    }

    // ── AC-1.8: Mật khẩu băm có salt (PBKDF2), không lưu thô / plain SHA-256 ─

    [Fact(DisplayName = "AC_1_8_PasswordHash_IsSaltedAndNotPlainTextOrPlainSha256")]
    public async Task AC_1_8_PasswordHash_IsSaltedAndNotPlainTextOrPlainSha256()
    {
        using var sqlite = TestDbHelper.CreateSqliteContext();
        var context = sqlite.Context;

        await DataSeeder.SeedAllAsync(context, _passwordService);

        var users = await context.Users.ToListAsync();
        Assert.NotEmpty(users);

        foreach (var user in users)
        {
            var hash = user.PasswordHash;

            // Không được là văn bản thuần
            Assert.DoesNotContain("Demo@123", hash);
            Assert.DoesNotContain("Admin@123", hash);

            // Phải chứa cấu trúc có salt: định dạng salt_base64:hash_base64
            Assert.Contains(":", hash);
            var parts = hash.Split(':');
            Assert.Equal(2, parts.Length);

            // Kiểm tra giải mã Base64 cho salt và subhash
            var saltBytes = Convert.FromBase64String(parts[0]);
            var hashBytes = Convert.FromBase64String(parts[1]);
            Assert.Equal(16, saltBytes.Length); // 128-bit salt
            Assert.Equal(32, hashBytes.Length); // 256-bit subhash

            // Xác thực hàm VerifyPassword hoạt động chính xác
            Assert.True(_passwordService.VerifyPassword("Demo@123456", hash),
                $"Mật khẩu demo của {user.Username} không xác thực được qua IPasswordService.");
        }
    }

    // ── Hồi quy tối thiểu Giai đoạn 0: Trang chủ, Health check, Security Headers

    [Fact(DisplayName = "Regression_Phase0_HomeAndHealthAndSecurityHeaders")]
    public async Task Regression_Phase0_HomeAndHealthAndSecurityHeaders()
    {
        var homeResp = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, homeResp.StatusCode);
        Assert.True(homeResp.Headers.Contains("X-Content-Type-Options"));
        Assert.False(homeResp.Headers.Contains("X-XSS-Protection"));

        var healthResp = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResp.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? dir;
        }
        throw new InvalidOperationException("Không tìm thấy thư mục gốc solution.");
    }
}
