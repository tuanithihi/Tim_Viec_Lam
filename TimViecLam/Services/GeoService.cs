using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ dữ liệu địa giới hành chính tối ưu bộ nhớ với IMemoryCache.
/// </summary>
public class GeoService : IGeoService
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    private const string CacheProvincesKey = "Geo_All_Provinces";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public GeoService(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<GeoItemDto>> GetProvincesAsync()
    {
        if (!_cache.TryGetValue(CacheProvincesKey, out List<GeoItemDto>? provinces) || provinces == null)
        {
            provinces = await _context.Provinces
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new GeoItemDto
                {
                    Id = p.ProvinceId,
                    Code = p.Code,
                    Name = p.Name,
                    FullName = p.FullName,
                    Slug = p.Slug
                })
                .ToListAsync();

            _cache.Set(CacheProvincesKey, provinces, CacheDuration);
        }

        return provinces;
    }

    public async Task<List<GeoItemDto>> GetDistrictsByProvinceIdAsync(int provinceId)
    {
        if (provinceId <= 0) return new List<GeoItemDto>();

        var cacheKey = $"Geo_Districts_Prov_{provinceId}";
        if (!_cache.TryGetValue(cacheKey, out List<GeoItemDto>? districts) || districts == null)
        {
            districts = await _context.Districts
                .AsNoTracking()
                .Where(d => d.ProvinceId == provinceId)
                .OrderBy(d => d.Name)
                .Select(d => new GeoItemDto
                {
                    Id = d.DistrictId,
                    Code = d.Code,
                    Name = d.Name,
                    FullName = d.FullName,
                    Slug = d.Slug
                })
                .ToListAsync();

            _cache.Set(cacheKey, districts, CacheDuration);
        }

        return districts;
    }

    public async Task<List<GeoItemDto>> GetWardsByDistrictIdAsync(int districtId)
    {
        if (districtId <= 0) return new List<GeoItemDto>();

        var cacheKey = $"Geo_Wards_Dist_{districtId}";
        if (!_cache.TryGetValue(cacheKey, out List<GeoItemDto>? wards) || wards == null)
        {
            wards = await _context.Wards
                .AsNoTracking()
                .Where(w => w.DistrictId == districtId)
                .OrderBy(w => w.Name)
                .Select(w => new GeoItemDto
                {
                    Id = w.WardId,
                    Code = w.Code,
                    Name = w.Name,
                    FullName = w.FullName,
                    Slug = w.Slug
                })
                .ToListAsync();

            _cache.Set(cacheKey, wards, CacheDuration);
        }

        return wards;
    }

    public async Task<PagedResult<LocationProvinceViewModel>> GetPagedProvincesAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 15;

        var query = _context.Provinces.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Code.Contains(s) || p.Slug.Contains(s));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new LocationProvinceViewModel
            {
                ProvinceId = p.ProvinceId,
                Code = p.Code,
                Name = p.Name,
                FullName = p.FullName,
                Slug = p.Slug,
                DistrictCount = p.Districts.Count,
                WorkplaceCount = p.Workplaces.Count
            })
            .ToListAsync();

        return new PagedResult<LocationProvinceViewModel>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}

