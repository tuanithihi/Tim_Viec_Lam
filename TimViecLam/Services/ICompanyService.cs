using Microsoft.AspNetCore.Http;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện xử lý quản lý hồ sơ công ty và quy trình kiểm duyệt công ty bởi SuperAdmin.
/// </summary>
public interface ICompanyService
{
    Task<CompanyProfileViewModel?> GetCompanyByIdAsync(int companyId);
    Task<CompanyProfileViewModel?> GetCompanyByUserIdAsync(int userId);
    Task<(bool Success, string? Message)> UpdateCompanyProfileAsync(int companyId, CompanyProfileInputModel model);
    Task<(bool Success, string? Message, string? LogoPath)> UploadLogoAsync(int companyId, IFormFile? file);
    Task<(bool Success, string? Message)> DeleteLogoAsync(int companyId);

    // ── Dành cho SuperAdmin kiểm duyệt ──────────────────────────────────
    Task<PagedResult<CompanyAdminViewModel>> GetPagedCompaniesAsync(string? search, string? status, int page = 1, int pageSize = 10);
    Task<(bool Success, string? Message)> ApproveCompanyAsync(int companyId);
    Task<(bool Success, string? Message)> RejectCompanyAsync(int companyId, string reason);
    Task<(bool Success, string? Message)> SuspendCompanyAsync(int companyId, string reason);
    Task<(bool Success, string? Message)> UnsuspendCompanyAsync(int companyId);
}

