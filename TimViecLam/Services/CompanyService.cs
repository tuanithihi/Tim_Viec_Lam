using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt dịch vụ quản lý thông tin công ty và quy trình phê duyệt của SuperAdmin.
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly DataContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        DataContext context,
        IFileUploadService fileUploadService,
        ILogger<CompanyService> logger)
    {
        _context = context;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<CompanyProfileViewModel?> GetCompanyByIdAsync(int companyId)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .Select(c => new CompanyProfileViewModel
            {
                CompanyId = c.CompanyId,
                UserId = c.UserId,
                Name = c.Name,
                Slug = c.Slug,
                Logo = c.Logo,
                Website = c.Website,
                TaxCode = c.TaxCode,
                CompanySize = c.CompanySize,
                Description = c.Description,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Status = c.Status,
                RejectReason = c.RejectReason,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                WorkplaceCount = c.Workplaces.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CompanyProfileViewModel?> GetCompanyByUserIdAsync(int userId)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new CompanyProfileViewModel
            {
                CompanyId = c.CompanyId,
                UserId = c.UserId,
                Name = c.Name,
                Slug = c.Slug,
                Logo = c.Logo,
                Website = c.Website,
                TaxCode = c.TaxCode,
                CompanySize = c.CompanySize,
                Description = c.Description,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Status = c.Status,
                RejectReason = c.RejectReason,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                WorkplaceCount = c.Workplaces.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string? Message)> UpdateCompanyProfileAsync(int companyId, CompanyProfileInputModel model)
    {
        if (model == null) return (false, "Dữ liệu không hợp lệ.");

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên công ty không được để trống.");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null)
        {
            return (false, "Không tìm thấy thông tin công ty.");
        }

        // Nếu đổi tên công ty, cập nhật lại Slug duy nhất
        string cleanName = model.Name.Trim();
        if (!string.Equals(company.Name, cleanName, StringComparison.OrdinalIgnoreCase))
        {
            string baseSlug = SlugHelper.Slugify(cleanName);
            string uniqueSlug = baseSlug;
            int counter = 1;

            while (await _context.Companies.AnyAsync(c => c.Slug == uniqueSlug && c.CompanyId != companyId))
            {
                uniqueSlug = $"{baseSlug}-{counter++}";
            }

            company.Name = cleanName;
            company.Slug = uniqueSlug;
        }

        // Cập nhật các trường thông tin cho phép (Chống Over-posting)
        company.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();
        company.TaxCode = string.IsNullOrWhiteSpace(model.TaxCode) ? null : model.TaxCode.Trim();
        company.CompanySize = string.IsNullOrWhiteSpace(model.CompanySize) ? "Small" : model.CompanySize.Trim();
        company.Description = model.Description;
        company.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        company.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        company.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
        company.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return (true, "Cập nhật hồ sơ công ty thành công.");
    }

    public async Task<(bool Success, string? Message, string? LogoPath)> UploadLogoAsync(int companyId, IFormFile? file)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null)
        {
            return (false, "Không tìm thấy công ty.", null);
        }

        var uploadResult = await _fileUploadService.UploadImageWithThumbnailAsync(
            file,
            targetFolder: "uploads/company/logos",
            thumbMaxWidth: 200,
            thumbMaxHeight: 200,
            maxFileSizeBytes: 2 * 1024 * 1024);

        if (!uploadResult.IsValid || string.IsNullOrEmpty(uploadResult.RelativePath))
        {
            return (false, uploadResult.ErrorMessage ?? "Tải file logo thất bại.", null);
        }

        // Xóa logo cũ nếu có
        if (!string.IsNullOrEmpty(company.Logo))
        {
            _fileUploadService.DeleteFile(company.Logo);
        }

        company.Logo = uploadResult.RelativePath;
        company.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return (true, "Cập nhật logo thành công.", uploadResult.RelativePath);
    }

    public async Task<(bool Success, string? Message)> DeleteLogoAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null)
        {
            return (false, "Không tìm thấy công ty.");
        }

        if (!string.IsNullOrEmpty(company.Logo))
        {
            _fileUploadService.DeleteFile(company.Logo);
            company.Logo = null;
            company.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return (true, "Đã xóa logo công ty.");
    }

    // ── SuperAdmin Moderation ───────────────────────────────────────────
    public async Task<PagedResult<CompanyAdminViewModel>> GetPagedCompaniesAsync(
        string? search,
        string? status,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Companies
            .Include(c => c.User)
            .Include(c => c.Workplaces)
            .Include(c => c.Jobs)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            string cleanStatus = status.Trim();
            query = query.Where(c => c.Status == cleanStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanSearch = search.Trim();
            string searchLower = cleanSearch.ToLower();
            query = query.Where(c =>
                c.Name.Contains(cleanSearch) ||
                (c.TaxCode != null && c.TaxCode.Contains(cleanSearch)) ||
                (c.Email != null && c.Email.Contains(cleanSearch)) ||
                (c.User != null && c.User.Username.Contains(cleanSearch)));
        }

        int totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyAdminViewModel
            {
                CompanyId = c.CompanyId,
                UserId = c.UserId,
                Username = c.User != null ? c.User.Username : string.Empty,
                UserEmail = c.User != null ? c.User.Email : string.Empty,
                Name = c.Name,
                Slug = c.Slug,
                Logo = c.Logo,
                Website = c.Website,
                TaxCode = c.TaxCode,
                CompanySize = c.CompanySize,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Status = c.Status,
                RejectReason = c.RejectReason,
                CreatedAt = c.CreatedAt,
                WorkplaceCount = c.Workplaces.Count,
                JobCount = c.Jobs.Count
            })
            .ToListAsync();

        return new PagedResult<CompanyAdminViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<(bool Success, string? Message)> ApproveCompanyAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null) return (false, "Không tìm thấy công ty.");

        company.Status = "Approved";
        company.RejectReason = null;
        company.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SuperAdmin đã duyệt công ty {CompanyId} ({Name})", company.CompanyId, company.Name);
        return (true, "Đã phê duyệt công ty thành công.");
    }

    public async Task<(bool Success, string? Message)> RejectCompanyAsync(int companyId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Lý do từ chối không được để trống.");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null) return (false, "Không tìm thấy công ty.");

        company.Status = "Rejected";
        company.RejectReason = reason.Trim();
        company.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SuperAdmin đã từ chối công ty {CompanyId} ({Name}) với lý do: {Reason}", company.CompanyId, company.Name, reason);
        return (true, "Đã từ chối công ty.");
    }

    public async Task<(bool Success, string? Message)> SuspendCompanyAsync(int companyId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Lý do tạm khóa không được để trống.");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null) return (false, "Không tìm thấy công ty.");

        company.Status = "Suspended";
        company.RejectReason = reason.Trim();
        company.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SuperAdmin đã tạm khóa công ty {CompanyId} ({Name}) với lý do: {Reason}", company.CompanyId, company.Name, reason);
        return (true, "Đã tạm khóa công ty.");
    }

    public async Task<(bool Success, string? Message)> UnsuspendCompanyAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null) return (false, "Không tìm thấy công ty.");

        if (company.Status != "Suspended")
        {
            return (false, "Công ty không ở trạng thái bị tạm khóa.");
        }

        company.Status = "Approved";
        company.RejectReason = null;
        company.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SuperAdmin đã mở khóa lại công ty {CompanyId} ({Name})", company.CompanyId, company.Name);
        return (true, "Đã mở khóa công ty.");
    }
}
