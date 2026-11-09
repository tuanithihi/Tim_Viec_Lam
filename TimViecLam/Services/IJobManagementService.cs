using TimViecLam.Models.ViewModels;

namespace TimViecLam.Services;

/// <summary>
/// Giao diện dịch vụ quản lý tin tuyển dụng phía Nhà tuyển dụng (CRUD, vòng đời, IDOR).
/// </summary>
public interface IJobManagementService
{
    /// <summary>
    /// Lấy danh sách tin tuyển dụng của công ty (phân trang, lọc theo trạng thái, tìm kiếm tiêu đề).
    /// </summary>
    Task<EmployerJobListViewModel> GetEmployerJobsAsync(int companyId, JobFilterModel filter);

    /// <summary>
    /// Lấy dữ liệu khởi tạo form tạo tin tuyển dụng (danh sách chi nhánh của công ty).
    /// </summary>
    Task<JobInputModel?> GetJobInputForCreateAsync(int companyId);

    /// <summary>
    /// Tạo mới tin tuyển dụng (chống Over-posting, validate lương/hạn nộp/kỹ năng, sanitize HTML, ghi AuditLog).
    /// </summary>
    Task<(bool Success, string? Message, int? JobId)> CreateJobAsync(int companyId, int userId, JobInputModel model);

    /// <summary>
    /// Lấy thông tin tin tuyển dụng để hiển thị form chỉnh sửa (chống IDOR).
    /// </summary>
    Task<JobInputModel?> GetJobInputForEditAsync(int companyId, int jobId);

    /// <summary>
    /// Cập nhật tin tuyển dụng (chống IDOR/Over-posting; sửa trường quan trọng của tin Published thì về Pending).
    /// </summary>
    Task<(bool Success, string? Message)> UpdateJobAsync(int companyId, int userId, int jobId, JobInputModel model);

    /// <summary>
    /// Gửi tin tuyển dụng đi phê duyệt (chuyển trạng thái từ Draft sang Pending hoặc Published nếu AutoApprove).
    /// </summary>
    Task<(bool Success, string? Message)> SendToApprovalAsync(int companyId, int userId, int jobId);

    /// <summary>
    /// Đóng tin tuyển dụng đang hiển thị (chuyển sang Closed).
    /// </summary>
    Task<(bool Success, string? Message)> CloseJobAsync(int companyId, int userId, int jobId);

    /// <summary>
    /// Mở lại tin tuyển dụng đã đóng (chuyển sang Pending hoặc Published nếu AutoApprove).
    /// </summary>
    Task<(bool Success, string? Message)> ReopenJobAsync(int companyId, int userId, int jobId);

    /// <summary>
    /// Nhân bản tin tuyển dụng thành bản nháp (Draft) mới.
    /// </summary>
    Task<(bool Success, string? Message, int? NewJobId)> DuplicateJobAsync(int companyId, int userId, int jobId);

    /// <summary>
    /// Xóa tin tuyển dụng (chặn xóa nếu đã có ứng viên nộp hồ sơ, chống IDOR).
    /// </summary>
    Task<(bool Success, string? Message)> DeleteJobAsync(int companyId, int userId, int jobId);
}

