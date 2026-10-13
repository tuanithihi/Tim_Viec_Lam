namespace TimViecLam.Services;

/// <summary>
/// Giao diện xử lý băm và kiểm tra mật khẩu bảo mật (có Salt + PBKDF2).
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Băm mật khẩu sử dụng thuật toán PBKDF2 có salt ngẫu nhiên.
    /// </summary>
    /// <param name="password">Mật khẩu thô.</param>
    /// <returns>Chuỗi mã hóa định dạng salt:hash (Base64).</returns>
    string HashPassword(string password);

    /// <summary>
    /// Kiểm tra mật khẩu thô với chuỗi băm lưu trữ.
    /// </summary>
    /// <param name="password">Mật khẩu thô cần kiểm tra.</param>
    /// <param name="passwordHash">Chuỗi băm lưu trữ.</param>
    /// <returns>True nếu mật khẩu khớp, ngược lại False.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
