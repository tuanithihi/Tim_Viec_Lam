namespace TimViecLam.Services;

/// <summary>
/// Giao diện xử lý băm, kiểm tra và xác thực chính sách mật khẩu bảo mật (sử dụng PasswordHasher và PBKDF2).
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Băm mật khẩu sử dụng PasswordHasher/PBKDF2 với salt ngẫu nhiên cho từng mật khẩu.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Kiểm tra mật khẩu thô với chuỗi băm lưu trữ.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Kiểm tra chính sách mật khẩu: tối thiểu 8 ký tự, chứa cả chữ cái và chữ số.
    /// </summary>
    bool ValidatePassword(string password, out string? errorMessage);
}
