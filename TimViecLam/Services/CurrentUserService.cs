using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt truy xuất thông tin người dùng từ HttpContextAccessor.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : null;
        }
    }

    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? CompanyId
    {
        get
        {
            var companyClaim = User?.FindFirst("CompanyId")?.Value;
            return int.TryParse(companyClaim, out int cid) ? cid : null;
        }
    }

    public string? CompanyStatus => User?.FindFirst("CompanyStatus")?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}

