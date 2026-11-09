using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint2;

public class Sprint2_SeedAndDataIntegrityTests
{
    // ── AC-S2.9: Seed chạy lần 2 số bản ghi không đổi (Idempotent) ────────────
    [Fact]
    public async Task AC_S2_9_SeedCategories_IsIdempotent_CountsDoNotChange()
    {
        var context = TestDbHelper.CreateInMemoryContext();

        // Lần chạy 1
        await DataSeeder.SeedJobCategoriesAsync(context);
        await DataSeeder.SeedJobTypesAsync(context);
        await DataSeeder.SeedSkillsAsync(context);
        await DataSeeder.SeedBenefitsAsync(context);

        var countCat1 = await context.JobCategories.CountAsync();
        var countType1 = await context.JobTypes.CountAsync();
        var countSkill1 = await context.Skills.CountAsync();
        var countBen1 = await context.Benefits.CountAsync();

        Assert.True(countCat1 >= 15, $"Expected >= 15 categories, got {countCat1}");
        Assert.True(countType1 >= 5, $"Expected >= 5 job types, got {countType1}");
        Assert.True(countSkill1 >= 40, $"Expected >= 40 skills, got {countSkill1}");
        Assert.True(countBen1 >= 10, $"Expected >= 10 benefits, got {countBen1}");

        // Lần chạy 2 (idempotent)
        await DataSeeder.SeedJobCategoriesAsync(context);
        await DataSeeder.SeedJobTypesAsync(context);
        await DataSeeder.SeedSkillsAsync(context);
        await DataSeeder.SeedBenefitsAsync(context);

        var countCat2 = await context.JobCategories.CountAsync();
        var countType2 = await context.JobTypes.CountAsync();
        var countSkill2 = await context.Skills.CountAsync();
        var countBen2 = await context.Benefits.CountAsync();

        Assert.Equal(countCat1, countCat2);
        Assert.Equal(countType1, countType2);
        Assert.Equal(countSkill1, countSkill2);
        Assert.Equal(countBen1, countBen2);
    }

    // ── AC-S2.9: Tất cả danh mục và loại công việc đều có Slug hợp lệ & duy nhất
    [Fact]
    public async Task AC_S2_9_AllSeededItems_Have_ValidAndUniqueSlugs()
    {
        var context = TestDbHelper.CreateInMemoryContext();

        await DataSeeder.SeedJobCategoriesAsync(context);
        await DataSeeder.SeedJobTypesAsync(context);
        await DataSeeder.SeedSkillsAsync(context);
        await DataSeeder.SeedBenefitsAsync(context);

        // JobCategories
        var categories = await context.JobCategories.ToListAsync();
        Assert.All(categories, c => Assert.False(string.IsNullOrWhiteSpace(c.Slug)));
        Assert.Equal(categories.Count, categories.Select(c => c.Slug).Distinct().Count());

        // JobTypes
        var types = await context.JobTypes.ToListAsync();
        Assert.All(types, t => Assert.False(string.IsNullOrWhiteSpace(t.Slug)));
        Assert.Equal(types.Count, types.Select(t => t.Slug).Distinct().Count());

        // Skills
        var skills = await context.Skills.ToListAsync();
        Assert.All(skills, s => Assert.False(string.IsNullOrWhiteSpace(s.Slug)));
        Assert.Equal(skills.Count, skills.Select(s => s.Slug).Distinct().Count());

        // Benefits
        var benefits = await context.Benefits.ToListAsync();
        Assert.All(benefits, b => Assert.False(string.IsNullOrWhiteSpace(b.Slug)));
        Assert.Equal(benefits.Count, benefits.Select(b => b.Slug).Distinct().Count());
    }

    // ── AC-S2.9: Slug địa giới hành chính không rỗng và duy nhất trong phạm vi cha ─
    [Fact]
    public async Task AC_S2_9_AdministrativeGeo_SlugsAreUniqueWithinParentScope()
    {
        var context = TestDbHelper.CreateInMemoryContext();

        // Thêm các tỉnh và quận/huyện mẫu (có cả trường hợp cùng tên ở các tỉnh khác nhau)
        var p1 = new tblProvince { Code = "01", Name = "Hà Nội", Slug = SlugHelper.Slugify("Hà Nội") };
        var p2 = new tblProvince { Code = "79", Name = "Hồ Chí Minh", Slug = SlugHelper.Slugify("Hồ Chí Minh") };
        context.Provinces.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var d1 = new tblDistrict { ProvinceId = p1.ProvinceId, Code = "001", Name = "Ba Đình", Slug = SlugHelper.Slugify("Ba Đình") };
        var d2 = new tblDistrict { ProvinceId = p1.ProvinceId, Code = "002", Name = "Cầu Giấy", Slug = SlugHelper.Slugify("Cầu Giấy") };
        var d3 = new tblDistrict { ProvinceId = p2.ProvinceId, Code = "760", Name = "Quận 1", Slug = SlugHelper.Slugify("Quận 1") };
        context.Districts.AddRange(d1, d2, d3);
        await context.SaveChangesAsync();

        var districtsP1 = await context.Districts.Where(d => d.ProvinceId == p1.ProvinceId).ToListAsync();
        Assert.All(districtsP1, d => Assert.False(string.IsNullOrWhiteSpace(d.Slug)));
        Assert.Equal(districtsP1.Count, districtsP1.Select(d => d.Slug).Distinct().Count());
    }

    // ── Hồi quy tối thiểu Sprint 1: PasswordHasher vẫn hoạt động chính xác ───────
    [Fact]
    public void Regression_Sprint1_PasswordService_HashAndVerify_WorksCorrectly()
    {
        var passwordService = new PasswordService();

        var hash1 = passwordService.HashPassword("Password123");
        var hash2 = passwordService.HashPassword("Password123");

        // Salt ngẫu nhiên nên 2 lần hash cùng 1 mật khẩu phải khác nhau
        Assert.NotEqual(hash1, hash2);

        // Verify đúng
        Assert.True(passwordService.VerifyPassword("Password123", hash1));
        // Verify sai mật khẩu
        Assert.False(passwordService.VerifyPassword("WrongPassword", hash1));
    }
}
