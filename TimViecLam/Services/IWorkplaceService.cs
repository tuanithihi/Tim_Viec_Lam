using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ quản lý địa điểm làm việc / chi nhánh của công ty (cô lập dữ liệu theo CompanyId).
/// </summary>
public interface IWorkplaceService
{
    Task<PagedResult<WorkplaceViewModel>> GetPagedWorkplacesAsync(int companyId, string? search, int page = 1, int pageSize = 10);
    Task<WorkplaceViewModel?> GetWorkplaceByIdAsync(int workplaceId, int companyId);
    Task<List<WorkplaceViewModel>> GetAllWorkplacesByCompanyAsync(int companyId);
    Task<(bool Success, string? Message, int WorkplaceId)> CreateWorkplaceAsync(int companyId, WorkplaceInputModel model);
    Task<(bool Success, string? Message)> UpdateWorkplaceAsync(int workplaceId, int companyId, WorkplaceInputModel model);
    Task<(bool Success, string? Message)> DeleteWorkplaceAsync(int workplaceId, int companyId);
}

