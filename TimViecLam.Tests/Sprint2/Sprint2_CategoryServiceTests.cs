using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;
using TimViecLam.Services;
using Xunit;

namespace TimViecLam.Tests.Sprint2;

public class Sprint2_CategoryServiceTests
{
    private (CategoryService service, DataContext context, IMemoryCache cache) CreateService()
    {
        var context = TestDbHelper.CreateInMemoryContext();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new CategoryService(context, cache);
        return (service, context, cache);
    }

    // ── AC-S2.2: Index tìm kiếm/phân trang đúng; <=3 truy vấn DB ─────────
    [Fact]
    public async Task AC_S2_2_Index_SearchAndPagination_ReturnsAccurateResults()
    {
        var (service, context, _) = CreateService();

        // Seed 15 categories
        for (int i = 1; i <= 15; i++)
        {
            context.JobCategories.Add(new tblJobCategory
            {
                Name = $"Ngành nghề số {i:D2}",
                Slug = $"nganh-nghe-so-{i:D2}",
                Description = i % 2 == 0 ? "Chuyên ngành kỹ thuật" : "Chuyên ngành kinh tế",
                SortOrder = i,
                IsActive = true
            });
        }
        await context.SaveChangesAsync();

        // 1. Phân trang trang 1 với pageSize 5
        var paged1 = await service.GetPagedCategoriesAsync(null, 1, 5);
        Assert.Equal(15, paged1.TotalItems);
        Assert.Equal(3, paged1.TotalPages);
        Assert.Equal(5, paged1.Items.Count);
        Assert.Equal("Ngành nghề số 01", paged1.Items[0].Name);

        // 2. Phân trang trang 3 với pageSize 5
        var paged3 = await service.GetPagedCategoriesAsync(null, 3, 5);
        Assert.Equal(5, paged3.Items.Count);
        Assert.Equal("Ngành nghề số 11", paged3.Items[0].Name);

        // 3. Tìm kiếm theo từ khóa
        var searchResult = await service.GetPagedCategoriesAsync("kỹ thuật", 1, 10);
        Assert.Equal(7, searchResult.TotalItems);
        Assert.All(searchResult.Items, item => Assert.Contains("kỹ thuật", item.Description, StringComparison.OrdinalIgnoreCase));
    }

    // ── AC-S2.3: Create: tên rỗng/trùng bị từ chối; Slug tự sinh, trùng thêm hậu tố ──
    [Fact]
    public async Task AC_S2_3_Create_EmptyOrDuplicateName_Rejected_AndSlugGetsSuffixWhenDuplicate()
    {
        var (service, context, _) = CreateService();

        // 1. Tên rỗng bị từ chối
        var emptyResult = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "   " });
        Assert.False(emptyResult.Success);
        Assert.Contains("không được để trống", emptyResult.Message);

        // 2. Thêm mới danh mục đầu tiên
        var okResult = await service.CreateCategoryAsync(new JobCategoryInputModel
        {
            Name = "Kế toán tổng hợp",
            Description = "Mô tả kế toán"
        });
        Assert.True(okResult.Success);

        // 3. Trùng tên (không phân biệt hoa thường và không phân biệt dấu) bị từ chối
        var dup1 = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "kế toán tổng hợp" });
        Assert.False(dup1.Success);
        Assert.Contains("đã tồn tại", dup1.Message);

        var dup2 = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "Ke toan tong hop" });
        Assert.False(dup2.Success);
        Assert.Contains("đã tồn tại", dup2.Message);

        // 4. Trùng Slug (tên khác nhau nhưng sinh ra slug giống nhau) được tự động thêm hậu tố -1, -2
        // "Lập trình C" và "Lập trình C#" có tên khác nhau (SearchTextBuilder giữ '#'), nhưng SlugHelper loại bỏ '#'
        var c1 = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "Lập trình C" });
        Assert.True(c1.Success);
        var item1 = await context.JobCategories.FindAsync(c1.Id);
        Assert.Equal("lap-trinh-c", item1!.Slug);

        var c2 = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "Lập trình C#" });
        Assert.True(c2.Success);
        var item2 = await context.JobCategories.FindAsync(c2.Id);
        Assert.Equal("lap-trinh-c-1", item2!.Slug);
    }

    // ── AC-S2.4: Edit: đổi tên cập nhật Slug; bật/tắt hoạt động đúng ─────
    [Fact]
    public async Task AC_S2_4_Edit_UpdatingNameUpdatesSlug_AndToggleActiveWorks()
    {
        var (service, context, _) = CreateService();

        var createRes = await service.CreateCategoryAsync(new JobCategoryInputModel
        {
            Name = "Lập trình web",
            SortOrder = 1
        });
        Assert.True(createRes.Success);

        // 1. Cập nhật đổi tên -> Slug đổi theo
        var editRes = await service.UpdateCategoryAsync(new JobCategoryInputModel
        {
            CategoryId = createRes.Id,
            Name = "Lập trình Web & Mobile",
            SortOrder = 2,
            IsActive = true
        });
        Assert.True(editRes.Success);

        var updated = await context.JobCategories.FindAsync(createRes.Id);
        Assert.Equal("Lập trình Web & Mobile", updated!.Name);
        Assert.Equal("lap-trinh-web-mobile", updated.Slug);

        // 2. Bật / Tắt hoạt động
        var toggle1 = await service.ToggleCategoryActiveAsync(createRes.Id);
        Assert.True(toggle1.Success);
        var disabled = await context.JobCategories.FindAsync(createRes.Id);
        Assert.False(disabled!.IsActive);

        var toggle2 = await service.ToggleCategoryActiveAsync(createRes.Id);
        Assert.True(toggle2.Success);
        var enabled = await context.JobCategories.FindAsync(createRes.Id);
        Assert.True(enabled!.IsActive);
    }

    // ── AC-S2.5: Delete danh mục đang dùng bị chặn kèm đề nghị tắt; danh mục không dùng xóa được ──
    [Fact]
    public async Task AC_S2_5_Delete_UsedCategoryIsBlocked_UnusedCategoryDeleted()
    {
        var (service, context, _) = CreateService();

        var catInUse = new tblJobCategory { Name = "Marketing", Slug = "marketing", IsActive = true };
        var catUnused = new tblJobCategory { Name = "Du lịch", Slug = "du-lich", IsActive = true };
        context.JobCategories.AddRange(catInUse, catUnused);
        await context.SaveChangesAsync();

        // Gán 1 job vào catInUse
        context.Jobs.Add(new tblJob
        {
            Title = "Chuyên viên SEO",
            Slug = "chuyen-vien-seo",
            CategoryId = catInUse.CategoryId,
            CompanyId = 1,
            JobTypeId = 1,
            WorkMode = "OnSite",
            Description = "Mô tả SEO",
            SearchText = "chuyen vien seo"
        });
        await context.SaveChangesAsync();

        // 1. Xóa danh mục đang dùng bị CHẶN và đề xuất tắt (AC-S2.5)
        var deleteUsedRes = await service.DeleteCategoryAsync(catInUse.CategoryId);
        Assert.False(deleteUsedRes.Success);
        Assert.Contains("vô hiệu hóa", deleteUsedRes.Message);
        Assert.NotNull(await context.JobCategories.FindAsync(catInUse.CategoryId));

        // 2. Xóa danh mục không dùng thành công
        var deleteUnusedRes = await service.DeleteCategoryAsync(catUnused.CategoryId);
        Assert.True(deleteUnusedRes.Success);
        Assert.Null(await context.JobCategories.FindAsync(catUnused.CategoryId));
    }

    // ── AC-S2.6: Danh mục đã tắt không xuất hiện trong bộ lọc công khai ───
    [Fact]
    public async Task AC_S2_6_DeactivatedCategories_DoNotAppearInPublicActiveList()
    {
        var (service, context, _) = CreateService();

        context.JobCategories.AddRange(
            new tblJobCategory { Name = "IT", Slug = "it", IsActive = true },
            new tblJobCategory { Name = "Nhân sự", Slug = "nhan-su", IsActive = false }
        );
        await context.SaveChangesAsync();

        var activeList = await service.GetActiveCategoriesAsync();
        Assert.Single(activeList);
        Assert.Equal("IT", activeList[0].Name);
        Assert.DoesNotContain(activeList, c => c.Name == "Nhân sự");
    }

    // ── AC-S2.7: Sau Create/Edit/Delete cache được làm mới ───────────────
    [Fact]
    public async Task AC_S2_7_Cache_RefreshedAfterCreateEditDelete()
    {
        var (service, context, cache) = CreateService();

        context.JobCategories.Add(new tblJobCategory { Name = "Thiết kế", Slug = "thiet-ke", IsActive = true });
        await context.SaveChangesAsync();

        // 1. Đọc lần đầu -> nạp vào cache
        var list1 = await service.GetActiveCategoriesAsync();
        Assert.Single(list1);
        Assert.True(cache.TryGetValue(CategoryService.CacheActiveCategories, out _));

        // 2. Tạo mới -> cache bị xóa, lần đọc sau lấy dữ liệu mới
        var createRes = await service.CreateCategoryAsync(new JobCategoryInputModel { Name = "Kiểm toán" });
        Assert.True(createRes.Success);
        Assert.False(cache.TryGetValue(CategoryService.CacheActiveCategories, out _)); // Đã bị xóa khỏi cache

        var list2 = await service.GetActiveCategoriesAsync();
        Assert.Equal(2, list2.Count);

        // 3. Edit / Tắt hoạt động -> cache bị xóa
        await service.ToggleCategoryActiveAsync(createRes.Id);
        Assert.False(cache.TryGetValue(CategoryService.CacheActiveCategories, out _));

        var list3 = await service.GetActiveCategoriesAsync();
        Assert.Single(list3);

        // 4. Delete -> cache bị xóa
        await service.DeleteCategoryAsync(createRes.Id);
        Assert.False(cache.TryGetValue(CategoryService.CacheActiveCategories, out _));
    }

    // ── Test phủ sóng cho JobType, Skill, Benefit ─────────────────────────
    [Fact]
    public async Task JobType_Skill_Benefit_CRUD_AndConstraintChecks_WorkCorrectly()
    {
        var (service, context, _) = CreateService();

        // 1. JobType
        var jtRes = await service.CreateJobTypeAsync(new JobTypeInputModel
        {
            Name = "Bán thời gian tối",
            Code = "PartTimeNight"
        });
        Assert.True(jtRes.Success);

        var jtActive = await service.GetActiveJobTypesAsync();
        Assert.Contains(jtActive, t => t.Code == "PartTimeNight");

        // 2. Skill
        var skRes = await service.CreateSkillAsync(new SkillInputModel
        {
            Name = "Golang Backend"
        });
        Assert.True(skRes.Success);

        var skActive = await service.GetActiveSkillsAsync();
        Assert.Contains(skActive, s => s.Name == "Golang Backend");

        // 3. Benefit
        var benRes = await service.CreateBenefitAsync(new BenefitInputModel
        {
            Name = "Phụ cấp đi lại xe buýt"
        });
        Assert.True(benRes.Success);

        var benActive = await service.GetActiveBenefitsAsync();
        Assert.Contains(benActive, b => b.Name == "Phụ cấp đi lại xe buýt");
    }
}

