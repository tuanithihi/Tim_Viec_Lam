using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ quản trị 4 danh mục dùng chung (Ngành nghề, Hình thức, Kỹ năng, Phúc lợi).
/// Hỗ trợ kiểm tra trùng lặp không dấu, sinh slug tự động kèm hậu tố, kiểm tra ràng buộc khi xóa và IMemoryCache.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public const string CacheActiveCategories = "Cache_Active_JobCategories";
    public const string CacheActiveJobTypes = "Cache_Active_JobTypes";
    public const string CacheActiveSkills = "Cache_Active_Skills";
    public const string CacheActiveBenefits = "Cache_Active_Benefits";

    public CategoryService(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // ═══════════════════════════════════════════════════════════════
    //  1. NGÀNH NGHỀ (JobCategory)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<JobCategoryViewModel>> GetPagedCategoriesAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.JobCategories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var unaccented = SearchTextBuilder.Build(s);
            query = query.Where(c => c.Name.Contains(s) || c.Slug.Contains(s) || (c.Description != null && c.Description.Contains(s)));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new JobCategoryViewModel
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Slug = c.Slug,
                Icon = c.Icon,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                JobCount = c.Jobs.Count
            })
            .ToListAsync();

        return new PagedResult<JobCategoryViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<JobCategoryInputModel?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.JobCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return null;

        return new JobCategoryInputModel
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Icon = category.Icon,
            Description = category.Description,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive
        };
    }

    public async Task<(bool Success, string Message, int Id)> CreateCategoryAsync(JobCategoryInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên ngành nghề không được để trống.", 0);
        }

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        // Kiểm tra trùng tên (không phân biệt hoa thường và không phân biệt dấu)
        var existingNames = await _context.JobCategories
            .AsNoTracking()
            .Select(c => c.Name)
            .ToListAsync();

        if (existingNames.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Tên ngành nghề \"{trimmedName}\" đã tồn tại trong hệ thống.", 0);
        }

        var slug = await GenerateUniqueCategorySlugAsync(trimmedName);

        var entity = new tblJobCategory
        {
            Name = trimmedName,
            Slug = slug,
            Icon = string.IsNullOrWhiteSpace(model.Icon) ? "bi-briefcase" : model.Icon.Trim(),
            Description = model.Description?.Trim(),
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        };

        _context.JobCategories.Add(entity);
        await _context.SaveChangesAsync();

        InvalidateCategoryCache();
        return (true, $"Đã thêm ngành nghề \"{entity.Name}\" thành công.", entity.CategoryId);
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(JobCategoryInputModel model)
    {
        var entity = await _context.JobCategories.FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId);
        if (entity == null)
        {
            return (false, "Không tìm thấy ngành nghề cần cập nhật.");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên ngành nghề không được để trống.");
        }

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        // Kiểm tra trùng tên với bản ghi khác
        var existingOtherNames = await _context.JobCategories
            .AsNoTracking()
            .Where(c => c.CategoryId != model.CategoryId)
            .Select(c => c.Name)
            .ToListAsync();

        if (existingOtherNames.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Tên ngành nghề \"{trimmedName}\" đã tồn tại trong hệ thống.");
        }

        // Nếu đổi tên thì cập nhật lại Slug
        if (!string.Equals(entity.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Slug = await GenerateUniqueCategorySlugAsync(trimmedName, entity.CategoryId);
        }

        entity.Name = trimmedName;
        entity.Icon = string.IsNullOrWhiteSpace(model.Icon) ? "bi-briefcase" : model.Icon.Trim();
        entity.Description = model.Description?.Trim();
        entity.SortOrder = model.SortOrder;
        entity.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        InvalidateCategoryCache();

        return (true, $"Đã cập nhật ngành nghề \"{entity.Name}\" thành công.");
    }

    public async Task<(bool Success, string Message)> ToggleCategoryActiveAsync(int id)
    {
        var entity = await _context.JobCategories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (entity == null) return (false, "Không tìm thấy ngành nghề.");

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        InvalidateCategoryCache();

        return (true, $"Đã {(entity.IsActive ? "bật kích hoạt" : "vô hiệu hóa")} ngành nghề \"{entity.Name}\".");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        var entity = await _context.JobCategories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (entity == null) return (false, "Không tìm thấy ngành nghề cần xóa.");

        // Kiểm tra xem ngành nghề có đang được tblJob sử dụng không (AC-S2.5)
        var isUsedInJobs = await _context.Jobs.AnyAsync(j => j.CategoryId == id);
        if (isUsedInJobs)
        {
            return (false, $"Không thể xóa ngành nghề \"{entity.Name}\" vì đang có tin tuyển dụng sử dụng. Bạn có thể vô hiệu hóa (tắt) ngành nghề này thay thế.");
        }

        _context.JobCategories.Remove(entity);
        await _context.SaveChangesAsync();
        InvalidateCategoryCache();

        return (true, $"Đã xóa ngành nghề \"{entity.Name}\" thành công.");
    }

    public async Task<List<JobCategoryViewModel>> GetActiveCategoriesAsync()
    {
        if (!_cache.TryGetValue(CacheActiveCategories, out List<JobCategoryViewModel>? list) || list == null)
        {
            list = await _context.JobCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new JobCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Icon = c.Icon,
                    Description = c.Description,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    JobCount = c.Jobs.Count
                })
                .ToListAsync();

            _cache.Set(CacheActiveCategories, list, TimeSpan.FromHours(1));
        }

        return list;
    }

    // ═══════════════════════════════════════════════════════════════
    //  2. HÌNH THỨC LÀM VIỆC (JobType)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<JobTypeViewModel>> GetPagedJobTypesAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.JobTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t => t.Name.Contains(s) || t.Code.Contains(s) || t.Slug.Contains(s));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new JobTypeViewModel
            {
                JobTypeId = t.JobTypeId,
                Name = t.Name,
                Code = t.Code,
                Slug = t.Slug,
                Description = t.Description,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive,
                JobCount = t.Jobs.Count
            })
            .ToListAsync();

        return new PagedResult<JobTypeViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<JobTypeInputModel?> GetJobTypeByIdAsync(int id)
    {
        var jobType = await _context.JobTypes.AsNoTracking().FirstOrDefaultAsync(t => t.JobTypeId == id);
        if (jobType == null) return null;

        return new JobTypeInputModel
        {
            JobTypeId = jobType.JobTypeId,
            Name = jobType.Name,
            Code = jobType.Code,
            Description = jobType.Description,
            SortOrder = jobType.SortOrder,
            IsActive = jobType.IsActive
        };
    }

    public async Task<(bool Success, string Message, int Id)> CreateJobTypeAsync(JobTypeInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên hình thức làm việc không được để trống.", 0);
        }

        if (string.IsNullOrWhiteSpace(model.Code))
        {
            return (false, "Mã hình thức làm việc không được để trống.", 0);
        }

        var trimmedName = model.Name.Trim();
        var trimmedCode = model.Code.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        // Kiểm tra trùng tên (không phân biệt hoa thường và dấu)
        var existingJobTypes = await _context.JobTypes.AsNoTracking().ToListAsync();
        if (existingJobTypes.Any(t => SearchTextBuilder.Build(t.Name) == normalizedNew))
        {
            return (false, $"Hình thức làm việc \"{trimmedName}\" đã tồn tại trong hệ thống.", 0);
        }

        if (existingJobTypes.Any(t => string.Equals(t.Code, trimmedCode, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, $"Mã hình thức \"{trimmedCode}\" đã được sử dụng.", 0);
        }

        var slug = await GenerateUniqueJobTypeSlugAsync(trimmedName);

        var entity = new tblJobType
        {
            Name = trimmedName,
            Code = trimmedCode,
            Slug = slug,
            Description = model.Description?.Trim(),
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        };

        _context.JobTypes.Add(entity);
        await _context.SaveChangesAsync();

        InvalidateJobTypeCache();
        return (true, $"Đã thêm hình thức làm việc \"{entity.Name}\" thành công.", entity.JobTypeId);
    }

    public async Task<(bool Success, string Message)> UpdateJobTypeAsync(JobTypeInputModel model)
    {
        var entity = await _context.JobTypes.FirstOrDefaultAsync(t => t.JobTypeId == model.JobTypeId);
        if (entity == null) return (false, "Không tìm thấy hình thức làm việc cần sửa.");

        if (string.IsNullOrWhiteSpace(model.Name)) return (false, "Tên hình thức không được để trống.");
        if (string.IsNullOrWhiteSpace(model.Code)) return (false, "Mã hình thức không được để trống.");

        var trimmedName = model.Name.Trim();
        var trimmedCode = model.Code.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        var existingOther = await _context.JobTypes
            .AsNoTracking()
            .Where(t => t.JobTypeId != model.JobTypeId)
            .ToListAsync();

        if (existingOther.Any(t => SearchTextBuilder.Build(t.Name) == normalizedNew))
        {
            return (false, $"Tên hình thức \"{trimmedName}\" đã tồn tại.");
        }

        if (existingOther.Any(t => string.Equals(t.Code, trimmedCode, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, $"Mã hình thức \"{trimmedCode}\" đã được sử dụng bởi hình thức khác.");
        }

        if (!string.Equals(entity.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Slug = await GenerateUniqueJobTypeSlugAsync(trimmedName, entity.JobTypeId);
        }

        entity.Name = trimmedName;
        entity.Code = trimmedCode;
        entity.Description = model.Description?.Trim();
        entity.SortOrder = model.SortOrder;
        entity.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        InvalidateJobTypeCache();

        return (true, $"Đã cập nhật hình thức làm việc \"{entity.Name}\" thành công.");
    }

    public async Task<(bool Success, string Message)> ToggleJobTypeActiveAsync(int id)
    {
        var entity = await _context.JobTypes.FirstOrDefaultAsync(t => t.JobTypeId == id);
        if (entity == null) return (false, "Không tìm thấy hình thức làm việc.");

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        InvalidateJobTypeCache();

        return (true, $"Đã {(entity.IsActive ? "bật kích hoạt" : "vô hiệu hóa")} hình thức \"{entity.Name}\".");
    }

    public async Task<(bool Success, string Message)> DeleteJobTypeAsync(int id)
    {
        var entity = await _context.JobTypes.FirstOrDefaultAsync(t => t.JobTypeId == id);
        if (entity == null) return (false, "Không tìm thấy hình thức làm việc.");

        var isUsed = await _context.Jobs.AnyAsync(j => j.JobTypeId == id);
        if (isUsed)
        {
            return (false, $"Không thể xóa hình thức \"{entity.Name}\" vì đang có tin tuyển dụng sử dụng. Bạn có thể vô hiệu hóa (tắt) hình thức này thay thế.");
        }

        _context.JobTypes.Remove(entity);
        await _context.SaveChangesAsync();
        InvalidateJobTypeCache();

        return (true, $"Đã xóa hình thức làm việc \"{entity.Name}\" thành công.");
    }

    public async Task<List<JobTypeViewModel>> GetActiveJobTypesAsync()
    {
        if (!_cache.TryGetValue(CacheActiveJobTypes, out List<JobTypeViewModel>? list) || list == null)
        {
            list = await _context.JobTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .Select(t => new JobTypeViewModel
                {
                    JobTypeId = t.JobTypeId,
                    Name = t.Name,
                    Code = t.Code,
                    Slug = t.Slug,
                    Description = t.Description,
                    SortOrder = t.SortOrder,
                    IsActive = t.IsActive,
                    JobCount = t.Jobs.Count
                })
                .ToListAsync();

            _cache.Set(CacheActiveJobTypes, list, TimeSpan.FromHours(1));
        }

        return list;
    }

    // ═══════════════════════════════════════════════════════════════
    //  3. KỸ NĂNG (Skill)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<SkillViewModel>> GetPagedSkillsAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Skills.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(sk => sk.Name.Contains(s) || sk.Slug.Contains(s));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(sk => sk.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sk => new SkillViewModel
            {
                SkillId = sk.SkillId,
                Name = sk.Name,
                Slug = sk.Slug,
                Description = sk.Description,
                IsActive = sk.IsActive,
                JobCount = sk.JobSkills.Count
            })
            .ToListAsync();

        return new PagedResult<SkillViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<SkillInputModel?> GetSkillByIdAsync(int id)
    {
        var skill = await _context.Skills.AsNoTracking().FirstOrDefaultAsync(sk => sk.SkillId == id);
        if (skill == null) return null;

        return new SkillInputModel
        {
            SkillId = skill.SkillId,
            Name = skill.Name,
            Description = skill.Description,
            IsActive = skill.IsActive
        };
    }

    public async Task<(bool Success, string Message, int Id)> CreateSkillAsync(SkillInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên kỹ năng không được để trống.", 0);
        }

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        var existingNames = await _context.Skills.AsNoTracking().Select(s => s.Name).ToListAsync();
        if (existingNames.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Kỹ năng \"{trimmedName}\" đã tồn tại trong hệ thống.", 0);
        }

        var slug = await GenerateUniqueSkillSlugAsync(trimmedName);

        var entity = new tblSkill
        {
            Name = trimmedName,
            Slug = slug,
            Description = model.Description?.Trim(),
            IsActive = model.IsActive
        };

        _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        InvalidateSkillCache();

        return (true, $"Đã thêm kỹ năng \"{entity.Name}\" thành công.", entity.SkillId);
    }

    public async Task<(bool Success, string Message)> UpdateSkillAsync(SkillInputModel model)
    {
        var entity = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == model.SkillId);
        if (entity == null) return (false, "Không tìm thấy kỹ năng cần sửa.");

        if (string.IsNullOrWhiteSpace(model.Name)) return (false, "Tên kỹ năng không được để trống.");

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        var existingOther = await _context.Skills
            .AsNoTracking()
            .Where(s => s.SkillId != model.SkillId)
            .Select(s => s.Name)
            .ToListAsync();

        if (existingOther.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Tên kỹ năng \"{trimmedName}\" đã tồn tại.");
        }

        if (!string.Equals(entity.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Slug = await GenerateUniqueSkillSlugAsync(trimmedName, entity.SkillId);
        }

        entity.Name = trimmedName;
        entity.Description = model.Description?.Trim();
        entity.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        InvalidateSkillCache();

        return (true, $"Đã cập nhật kỹ năng \"{entity.Name}\" thành công.");
    }

    public async Task<(bool Success, string Message)> ToggleSkillActiveAsync(int id)
    {
        var entity = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == id);
        if (entity == null) return (false, "Không tìm thấy kỹ năng.");

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        InvalidateSkillCache();

        return (true, $"Đã {(entity.IsActive ? "bật kích hoạt" : "vô hiệu hóa")} kỹ năng \"{entity.Name}\".");
    }

    public async Task<(bool Success, string Message)> DeleteSkillAsync(int id)
    {
        var entity = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == id);
        if (entity == null) return (false, "Không tìm thấy kỹ năng.");

        var isUsed = await _context.JobSkills.AnyAsync(js => js.SkillId == id);
        if (isUsed)
        {
            return (false, $"Không thể xóa kỹ năng \"{entity.Name}\" vì đang được gán cho tin tuyển dụng. Bạn có thể vô hiệu hóa (tắt) kỹ năng này thay thế.");
        }

        _context.Skills.Remove(entity);
        await _context.SaveChangesAsync();
        InvalidateSkillCache();

        return (true, $"Đã xóa kỹ năng \"{entity.Name}\" thành công.");
    }

    public async Task<List<SkillViewModel>> GetActiveSkillsAsync()
    {
        if (!_cache.TryGetValue(CacheActiveSkills, out List<SkillViewModel>? list) || list == null)
        {
            list = await _context.Skills
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SkillViewModel
                {
                    SkillId = s.SkillId,
                    Name = s.Name,
                    Slug = s.Slug,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    JobCount = s.JobSkills.Count
                })
                .ToListAsync();

            _cache.Set(CacheActiveSkills, list, TimeSpan.FromHours(1));
        }

        return list;
    }

    // ═══════════════════════════════════════════════════════════════
    //  4. PHÚC LỢI (Benefit)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<BenefitViewModel>> GetPagedBenefitsAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Benefits.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(b => b.Name.Contains(s) || b.Slug.Contains(s));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BenefitViewModel
            {
                BenefitId = b.BenefitId,
                Name = b.Name,
                Slug = b.Slug,
                Icon = b.Icon,
                Description = b.Description,
                IsActive = b.IsActive,
                JobCount = b.JobBenefits.Count
            })
            .ToListAsync();

        return new PagedResult<BenefitViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<BenefitInputModel?> GetBenefitByIdAsync(int id)
    {
        var benefit = await _context.Benefits.AsNoTracking().FirstOrDefaultAsync(b => b.BenefitId == id);
        if (benefit == null) return null;

        return new BenefitInputModel
        {
            BenefitId = benefit.BenefitId,
            Name = benefit.Name,
            Icon = benefit.Icon,
            Description = benefit.Description,
            IsActive = benefit.IsActive
        };
    }

    public async Task<(bool Success, string Message, int Id)> CreateBenefitAsync(BenefitInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên phúc lợi không được để trống.", 0);
        }

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        var existingNames = await _context.Benefits.AsNoTracking().Select(b => b.Name).ToListAsync();
        if (existingNames.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Phúc lợi \"{trimmedName}\" đã tồn tại trong hệ thống.", 0);
        }

        var slug = await GenerateUniqueBenefitSlugAsync(trimmedName);

        var entity = new tblBenefit
        {
            Name = trimmedName,
            Slug = slug,
            Icon = string.IsNullOrWhiteSpace(model.Icon) ? "bi-gift" : model.Icon.Trim(),
            Description = model.Description?.Trim(),
            IsActive = model.IsActive
        };

        _context.Benefits.Add(entity);
        await _context.SaveChangesAsync();
        InvalidateBenefitCache();

        return (true, $"Đã thêm phúc lợi \"{entity.Name}\" thành công.", entity.BenefitId);
    }

    public async Task<(bool Success, string Message)> UpdateBenefitAsync(BenefitInputModel model)
    {
        var entity = await _context.Benefits.FirstOrDefaultAsync(b => b.BenefitId == model.BenefitId);
        if (entity == null) return (false, "Không tìm thấy phúc lợi cần sửa.");

        if (string.IsNullOrWhiteSpace(model.Name)) return (false, "Tên phúc lợi không được để trống.");

        var trimmedName = model.Name.Trim();
        var normalizedNew = SearchTextBuilder.Build(trimmedName);

        var existingOther = await _context.Benefits
            .AsNoTracking()
            .Where(b => b.BenefitId != model.BenefitId)
            .Select(b => b.Name)
            .ToListAsync();

        if (existingOther.Any(n => SearchTextBuilder.Build(n) == normalizedNew))
        {
            return (false, $"Tên phúc lợi \"{trimmedName}\" đã tồn tại.");
        }

        if (!string.Equals(entity.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Slug = await GenerateUniqueBenefitSlugAsync(trimmedName, entity.BenefitId);
        }

        entity.Name = trimmedName;
        entity.Icon = string.IsNullOrWhiteSpace(model.Icon) ? "bi-gift" : model.Icon.Trim();
        entity.Description = model.Description?.Trim();
        entity.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        InvalidateBenefitCache();

        return (true, $"Đã cập nhật phúc lợi \"{entity.Name}\" thành công.");
    }

    public async Task<(bool Success, string Message)> ToggleBenefitActiveAsync(int id)
    {
        var entity = await _context.Benefits.FirstOrDefaultAsync(b => b.BenefitId == id);
        if (entity == null) return (false, "Không tìm thấy phúc lợi.");

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        InvalidateBenefitCache();

        return (true, $"Đã {(entity.IsActive ? "bật kích hoạt" : "vô hiệu hóa")} phúc lợi \"{entity.Name}\".");
    }

    public async Task<(bool Success, string Message)> DeleteBenefitAsync(int id)
    {
        var entity = await _context.Benefits.FirstOrDefaultAsync(b => b.BenefitId == id);
        if (entity == null) return (false, "Không tìm thấy phúc lợi.");

        var isUsed = await _context.JobBenefits.AnyAsync(jb => jb.BenefitId == id);
        if (isUsed)
        {
            return (false, $"Không thể xóa phúc lợi \"{entity.Name}\" vì đang được gán cho tin tuyển dụng. Bạn có thể vô hiệu hóa (tắt) phúc lợi này thay thế.");
        }

        _context.Benefits.Remove(entity);
        await _context.SaveChangesAsync();
        InvalidateBenefitCache();

        return (true, $"Đã xóa phúc lợi \"{entity.Name}\" thành công.");
    }

    public async Task<List<BenefitViewModel>> GetActiveBenefitsAsync()
    {
        if (!_cache.TryGetValue(CacheActiveBenefits, out List<BenefitViewModel>? list) || list == null)
        {
            list = await _context.Benefits
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new BenefitViewModel
                {
                    BenefitId = b.BenefitId,
                    Name = b.Name,
                    Slug = b.Slug,
                    Icon = b.Icon,
                    Description = b.Description,
                    IsActive = b.IsActive,
                    JobCount = b.JobBenefits.Count
                })
                .ToListAsync();

            _cache.Set(CacheActiveBenefits, list, TimeSpan.FromHours(1));
        }

        return list;
    }

    // ═══════════════════════════════════════════════════════════════
    //  5. TIỆN ÍCH HELPER SINH SLUG DUY NHẤT & XÓA CACHE
    // ═══════════════════════════════════════════════════════════════

    public void InvalidateCategoryCache() => _cache.Remove(CacheActiveCategories);
    public void InvalidateJobTypeCache() => _cache.Remove(CacheActiveJobTypes);
    public void InvalidateSkillCache() => _cache.Remove(CacheActiveSkills);
    public void InvalidateBenefitCache() => _cache.Remove(CacheActiveBenefits);

    private async Task<string> GenerateUniqueCategorySlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "nganh-nghe";
        var slug = baseSlug;
        int counter = 1;
        while (await _context.JobCategories.AnyAsync(c => c.Slug == slug && (!excludeId.HasValue || c.CategoryId != excludeId.Value)))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }

    private async Task<string> GenerateUniqueJobTypeSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "hinh-thuc";
        var slug = baseSlug;
        int counter = 1;
        while (await _context.JobTypes.AnyAsync(t => t.Slug == slug && (!excludeId.HasValue || t.JobTypeId != excludeId.Value)))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }

    private async Task<string> GenerateUniqueSkillSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "ky-nang";
        var slug = baseSlug;
        int counter = 1;
        while (await _context.Skills.AnyAsync(s => s.Slug == slug && (!excludeId.HasValue || s.SkillId != excludeId.Value)))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }

    private async Task<string> GenerateUniqueBenefitSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "phuc-loi";
        var slug = baseSlug;
        int counter = 1;
        while (await _context.Benefits.AnyAsync(b => b.Slug == slug && (!excludeId.HasValue || b.BenefitId != excludeId.Value)))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }
}

