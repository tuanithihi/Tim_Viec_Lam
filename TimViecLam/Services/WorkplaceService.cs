using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt dịch vụ quản lý địa điểm làm việc, kiểm tra tính toàn vẹn quan hệ cha-con địa giới và tọa độ trong lãnh thổ Việt Nam.
/// </summary>
public class WorkplaceService : IWorkplaceService
{
    private readonly DataContext _context;
    private readonly ILogger<WorkplaceService> _logger;

    public WorkplaceService(DataContext context, ILogger<WorkplaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<WorkplaceViewModel>> GetPagedWorkplacesAsync(
        int companyId,
        string? search,
        int page = 1,
        int pageSize = 10)
    {
        // Truy vấn phân trang tối ưu (1 đếm + 1 lấy trang = 2 queries <= 3 queries)
        var query = _context.Workplaces
            .AsNoTracking()
            .Include(w => w.Province)
            .Include(w => w.District)
            .Include(w => w.Ward)
            .Where(w => w.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanSearch = search.Trim();
            query = query.Where(w =>
                w.Name.Contains(cleanSearch) ||
                w.Address.Contains(cleanSearch) ||
                (w.Province != null && w.Province.Name.Contains(cleanSearch)) ||
                (w.District != null && w.District.Name.Contains(cleanSearch)));
        }

        int totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.IsHeadOffice)
            .ThenByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkplaceViewModel
            {
                WorkplaceId = w.WorkplaceId,
                CompanyId = w.CompanyId,
                Name = w.Name,
                Address = w.Address,
                ProvinceId = w.ProvinceId,
                ProvinceName = w.Province != null ? w.Province.Name : string.Empty,
                DistrictId = w.DistrictId,
                DistrictName = w.District != null ? w.District.Name : string.Empty,
                WardId = w.WardId,
                WardName = w.Ward != null ? w.Ward.Name : null,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                IsHeadOffice = w.IsHeadOffice,
                CreatedAt = w.CreatedAt,
                JobCount = w.Jobs.Count
            })
            .ToListAsync();

        return new PagedResult<WorkplaceViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<WorkplaceViewModel?> GetWorkplaceByIdAsync(int workplaceId, int companyId)
    {
        // Đảm bảo cô lập tuyệt đối: chỉ lấy địa điểm của chính công ty này (chống IDOR)
        return await _context.Workplaces
            .AsNoTracking()
            .Include(w => w.Province)
            .Include(w => w.District)
            .Include(w => w.Ward)
            .Where(w => w.WorkplaceId == workplaceId && w.CompanyId == companyId)
            .Select(w => new WorkplaceViewModel
            {
                WorkplaceId = w.WorkplaceId,
                CompanyId = w.CompanyId,
                Name = w.Name,
                Address = w.Address,
                ProvinceId = w.ProvinceId,
                ProvinceName = w.Province != null ? w.Province.Name : string.Empty,
                DistrictId = w.DistrictId,
                DistrictName = w.District != null ? w.District.Name : string.Empty,
                WardId = w.WardId,
                WardName = w.Ward != null ? w.Ward.Name : null,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                IsHeadOffice = w.IsHeadOffice,
                CreatedAt = w.CreatedAt,
                JobCount = w.Jobs.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkplaceViewModel>> GetAllWorkplacesByCompanyAsync(int companyId)
    {
        return await _context.Workplaces
            .AsNoTracking()
            .Include(w => w.Province)
            .Include(w => w.District)
            .Where(w => w.CompanyId == companyId)
            .OrderByDescending(w => w.IsHeadOffice)
            .ThenBy(w => w.Name)
            .Select(w => new WorkplaceViewModel
            {
                WorkplaceId = w.WorkplaceId,
                CompanyId = w.CompanyId,
                Name = w.Name,
                Address = w.Address,
                ProvinceId = w.ProvinceId,
                ProvinceName = w.Province != null ? w.Province.Name : string.Empty,
                DistrictId = w.DistrictId,
                DistrictName = w.District != null ? w.District.Name : string.Empty,
                WardId = w.WardId,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                IsHeadOffice = w.IsHeadOffice
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string? Message, int WorkplaceId)> CreateWorkplaceAsync(int companyId, WorkplaceInputModel model)
    {
        var validation = await ValidateWorkplaceModelAsync(model);
        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage, 0);
        }

        // Nếu đặt làm trụ sở chính, bỏ trạng thái trụ sở chính của các địa điểm khác trong công ty
        if (model.IsHeadOffice)
        {
            var otherHeadOffices = await _context.Workplaces
                .Where(w => w.CompanyId == companyId && w.IsHeadOffice)
                .ToListAsync();
            foreach (var ho in otherHeadOffices)
            {
                ho.IsHeadOffice = false;
            }
        }

        var workplace = new tblWorkplace
        {
            CompanyId = companyId, // Luôn gán từ session, không nhận từ client
            Name = model.Name.Trim(),
            Address = model.Address.Trim(),
            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            WardId = model.WardId,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            IsHeadOffice = model.IsHeadOffice,
            CreatedAt = DateTime.Now
        };

        _context.Workplaces.Add(workplace);
        await _context.SaveChangesAsync();

        return (true, "Thêm địa điểm làm việc thành công.", workplace.WorkplaceId);
    }

    public async Task<(bool Success, string? Message)> UpdateWorkplaceAsync(int workplaceId, int companyId, WorkplaceInputModel model)
    {
        // Chống IDOR: Chỉ tìm địa điểm thuộc công ty của người dùng hiện tại
        var workplace = await _context.Workplaces
            .FirstOrDefaultAsync(w => w.WorkplaceId == workplaceId && w.CompanyId == companyId);

        if (workplace == null)
        {
            return (false, "Không tìm thấy địa điểm làm việc hoặc bạn không có quyền sửa.");
        }

        var validation = await ValidateWorkplaceModelAsync(model);
        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage);
        }

        if (model.IsHeadOffice && !workplace.IsHeadOffice)
        {
            var otherHeadOffices = await _context.Workplaces
                .Where(w => w.CompanyId == companyId && w.IsHeadOffice && w.WorkplaceId != workplaceId)
                .ToListAsync();
            foreach (var ho in otherHeadOffices)
            {
                ho.IsHeadOffice = false;
            }
        }

        workplace.Name = model.Name.Trim();
        workplace.Address = model.Address.Trim();
        workplace.ProvinceId = model.ProvinceId;
        workplace.DistrictId = model.DistrictId;
        workplace.WardId = model.WardId;
        workplace.Latitude = model.Latitude;
        workplace.Longitude = model.Longitude;
        workplace.IsHeadOffice = model.IsHeadOffice;

        await _context.SaveChangesAsync();
        return (true, "Cập nhật địa điểm làm việc thành công.");
    }

    public async Task<(bool Success, string? Message)> DeleteWorkplaceAsync(int workplaceId, int companyId)
    {
        // Chống IDOR: kiểm tra quyền sở hữu
        var workplace = await _context.Workplaces
            .FirstOrDefaultAsync(w => w.WorkplaceId == workplaceId && w.CompanyId == companyId);

        if (workplace == null)
        {
            return (false, "Không tìm thấy địa điểm làm việc hoặc bạn không có quyền xóa.");
        }

        // Chặn xóa nếu đang có tin tuyển dụng sử dụng địa điểm này (AC-S3A.6)
        bool hasLinkedJobs = await _context.Jobs.AnyAsync(j => j.WorkplaceId == workplaceId);
        if (hasLinkedJobs)
        {
            return (false, "Không thể xóa địa điểm này vì đang có tin tuyển dụng liên kết.");
        }

        _context.Workplaces.Remove(workplace);
        await _context.SaveChangesAsync();

        return (true, "Đã xóa địa điểm làm việc.");
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateWorkplaceModelAsync(WorkplaceInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "Tên địa điểm không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(model.Address))
        {
            return (false, "Địa chỉ chi tiết không được để trống.");
        }

        // 1. Kiểm tra tọa độ trong phạm vi lãnh thổ Việt Nam (Lat 8–24, Lng 102–110)
        if (model.Latitude.HasValue || model.Longitude.HasValue)
        {
            if (!model.Latitude.HasValue || !model.Longitude.HasValue)
            {
                return (false, "Vui lòng nhập cả Vĩ độ và Kinh độ.");
            }

            double lat = model.Latitude.Value;
            double lng = model.Longitude.Value;

            if (lat < 8.0 || lat > 24.0 || lng < 102.0 || lng > 110.0)
            {
                return (false, "Tọa độ không hợp lệ hoặc nằm ngoài phạm vi lãnh thổ Việt Nam (Vĩ độ: 8–24, Kinh độ: 102–110).");
            }
        }

        // 2. Kiểm tra quan hệ Quận/Huyện phải thuộc Tỉnh/Thành phố
        var district = await _context.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.DistrictId == model.DistrictId);
        if (district == null || district.ProvinceId != model.ProvinceId)
        {
            return (false, "Quận/Huyện được chọn không thuộc Tỉnh/Thành phố đã chọn.");
        }

        // 3. Kiểm tra quan hệ Phường/Xã phải thuộc Quận/Huyện (nếu có chọn)
        if (model.WardId.HasValue && model.WardId.Value > 0)
        {
            var ward = await _context.Wards.AsNoTracking().FirstOrDefaultAsync(w => w.WardId == model.WardId.Value);
            if (ward == null || ward.DistrictId != model.DistrictId)
            {
                return (false, "Phường/Xã được chọn không thuộc Quận/Huyện đã chọn.");
            }
        }

        return (true, null);
    }
}

