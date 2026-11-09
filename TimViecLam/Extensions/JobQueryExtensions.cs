using TimViecLam.Models;

namespace TimViecLam.Extensions;

/// <summary>
/// Các phương thức mở rộng truy vấn Linq cho thực thể Việc làm (tblJob).
/// </summary>
public static class JobQueryExtensions
{
    /// <summary>
    /// Lọc các tin tuyển dụng hợp lệ để hiển thị công khai cho ứng viên:
    /// 1. Trạng thái đã duyệt xuất bản: ApprovalStatus == "Published"
    /// 2. Còn hạn nộp hồ sơ: ExpiresAt > DateTime.Now
    /// 3. Công ty chủ quản ở trạng thái đã được phê duyệt: Company != null && Company.Status == "Approved"
    /// </summary>
    public static IQueryable<tblJob> WhereVisibleToPublic(this IQueryable<tblJob> query)
    {
        var now = DateTime.Now;
        return query.Where(j => j.ApprovalStatus == "Published"
                             && j.ExpiresAt > now
                             && j.Company != null
                             && j.Company.Status == "Approved");
    }
}

