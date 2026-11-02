namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ trích xuất thông tin định danh của người dùng hiện tại từ ClaimsPrincipal.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Mã người dùng hiện tại (null nếu chưa đăng nhập).
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Tên đăng nhập.
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Địa chỉ Email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Vai trò (SuperAdmin | Employer | Candidate).
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Mã công ty gắn với tài khoản Nhà tuyển dụng (nếu có).
    /// </summary>
    int? CompanyId { get; }

    /// <summary>
    /// Trạng thái phê duyệt của công ty (Pending | Approved | Suspended | Rejected).
    /// </summary>
    string? CompanyStatus { get; }

    /// <summary>
    /// Người dùng đã đăng nhập hay chưa.
    /// </summary>
    bool IsAuthenticated { get; }
}
