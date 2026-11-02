using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using TimViecLam.Models;

namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ băm mật khẩu bảo mật sử dụng Microsoft.AspNetCore.Identity.PasswordHasher với PBKDF2 và salt ngẫu nhiên.
/// Đảm bảo: hai người dùng cùng mật khẩu sẽ sinh hai hash khác nhau; không dùng SHA-256 không salt.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<tblUser> _hasher = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Mật khẩu không được để trống.", nameof(password));
        }

        return _hasher.HashPassword(new tblUser(), password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            // Hỗ trợ kiểm tra legacy format (salt_base64:hash_base64) nếu có
            if (passwordHash.Contains(':'))
            {
                var parts = passwordHash.Split(':');
                if (parts.Length == 2)
                {
                    byte[] salt = Convert.FromBase64String(parts[0]);
                    byte[] expectedHash = Convert.FromBase64String(parts[1]);

                    byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        100_000,
                        HashAlgorithmName.SHA256,
                        32);

                    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
                }
            }

            var result = _hasher.VerifyHashedPassword(new tblUser(), passwordHash, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatePassword(string password, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            errorMessage = "Mật khẩu phải có tối thiểu 8 ký tự.";
            return false;
        }

        bool hasLetter = password.Any(char.IsLetter);
        bool hasDigit = password.Any(char.IsDigit);

        if (!hasLetter || !hasDigit)
        {
            errorMessage = "Mật khẩu phải chứa ít nhất một chữ cái và một chữ số.";
            return false;
        }

        return true;
    }
}
