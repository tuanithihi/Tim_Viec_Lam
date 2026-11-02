namespace TimViecLam.Services;

/// <summary>
/// Dịch vụ gửi email thông báo hệ thống.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Gửi email bất đồng bộ.
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}
