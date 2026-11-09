using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ quản lý 4 danh mục dùng chung (Ngành nghề, Hình thức làm việc, Kỹ năng, Phúc lợi).
/// </summary>
public interface ICategoryService
{
    // ── Ngành nghề (JobCategory) ───────────────────────────────────────────
    Task<PagedResult<JobCategoryViewModel>> GetPagedCategoriesAsync(string? search, int page, int pageSize);
    Task<JobCategoryInputModel?> GetCategoryByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateCategoryAsync(JobCategoryInputModel model);
    Task<(bool Success, string Message)> UpdateCategoryAsync(JobCategoryInputModel model);
    Task<(bool Success, string Message)> ToggleCategoryActiveAsync(int id);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
    Task<List<JobCategoryViewModel>> GetActiveCategoriesAsync();

    // ── Hình thức làm việc (JobType) ───────────────────────────────────────
    Task<PagedResult<JobTypeViewModel>> GetPagedJobTypesAsync(string? search, int page, int pageSize);
    Task<JobTypeInputModel?> GetJobTypeByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateJobTypeAsync(JobTypeInputModel model);
    Task<(bool Success, string Message)> UpdateJobTypeAsync(JobTypeInputModel model);
    Task<(bool Success, string Message)> ToggleJobTypeActiveAsync(int id);
    Task<(bool Success, string Message)> DeleteJobTypeAsync(int id);
    Task<List<JobTypeViewModel>> GetActiveJobTypesAsync();

    // ── Kỹ năng (Skill) ────────────────────────────────────────────────────
    Task<PagedResult<SkillViewModel>> GetPagedSkillsAsync(string? search, int page, int pageSize);
    Task<SkillInputModel?> GetSkillByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateSkillAsync(SkillInputModel model);
    Task<(bool Success, string Message)> UpdateSkillAsync(SkillInputModel model);
    Task<(bool Success, string Message)> ToggleSkillActiveAsync(int id);
    Task<(bool Success, string Message)> DeleteSkillAsync(int id);
    Task<List<SkillViewModel>> GetActiveSkillsAsync();

    // ── Phúc lợi (Benefit) ─────────────────────────────────────────────────
    Task<PagedResult<BenefitViewModel>> GetPagedBenefitsAsync(string? search, int page, int pageSize);
    Task<BenefitInputModel?> GetBenefitByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateBenefitAsync(BenefitInputModel model);
    Task<(bool Success, string Message)> UpdateBenefitAsync(BenefitInputModel model);
    Task<(bool Success, string Message)> ToggleBenefitActiveAsync(int id);
    Task<(bool Success, string Message)> DeleteBenefitAsync(int id);
    Task<List<BenefitViewModel>> GetActiveBenefitsAsync();

    // ── Cache Invalidation ────────────────────────────────────────────────
    void InvalidateCategoryCache();
    void InvalidateJobTypeCache();
    void InvalidateSkillCache();
    void InvalidateBenefitCache();
}

