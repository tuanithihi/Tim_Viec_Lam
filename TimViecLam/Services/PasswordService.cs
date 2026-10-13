using System.Security.Cryptography;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ băm mật khẩu bảo mật sử dụng PBKDF2 với HMAC-SHA256 và Salt 128-bit.
/// Đảm bảo không lưu mật khẩu thô và không dùng SHA-256 không salt.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Mật khẩu không được để trống.", nameof(password));
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        // Định dạng lưu trữ: salt_base64:hash_base64
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithm,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
