using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ kiểm duyệt tin tuyển dụng phía SuperAdmin.
/// </summary>
public interface IJobModerationService
{
    /// <summary>
    /// Lấy danh sách tin tuyển dụng cần kiểm duyệt (phân trang, lọc, tìm kiếm).
    /// </summary>
    Task<JobModerationListViewModel> GetPendingJobsAsync(JobFilterModel filter);

    /// <summary>
    /// Lấy chi tiết tin tuyển dụng phục vụ SuperAdmin xem xét trước khi duyệt.
    /// </summary>
    Task<JobModerationDetailsViewModel?> GetJobDetailsForModerationAsync(int jobId);

    /// <summary>
    /// Phê duyệt tin tuyển dụng (chuyển sang Published, gán PublishedAt, ghi AuditLog).
    /// </summary>
    Task<(bool Success, string? Message)> ApproveJobAsync(int adminUserId, int jobId);

    /// <summary>
    /// Từ chối tin tuyển dụng (bắt buộc nhập lý do, chuyển sang Rejected, ghi AuditLog).
    /// </summary>
    Task<(bool Success, string? Message)> RejectJobAsync(int adminUserId, int jobId, string reason);

    /// <summary>
    /// Gỡ bỏ tin tuyển dụng đang xuất bản (bắt buộc nhập lý do, chuyển sang Closed, ghi AuditLog).
    /// </summary>
    Task<(bool Success, string? Message)> UnpublishJobAsync(int adminUserId, int jobId, string reason);

    /// <summary>
    /// Phê duyệt hàng loạt các tin tuyển dụng đang chờ duyệt (bỏ qua các tin không ở trạng thái Pending).
    /// </summary>
    Task<(bool Success, string? Message, int ApprovedCount)> BatchApproveJobsAsync(int adminUserId, List<int> jobIds);
}

