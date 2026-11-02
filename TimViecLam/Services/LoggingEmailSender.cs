using Microsoft.Extensions.Logging;

namespace TimViecLam.Services;

/// <summary>
/// Cài đặt IEmailSender mặc định ghi log liên kết vào Console / Logger (không gửi email thực ở môi trường phát triển).
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        _logger.LogInformation("========== [GỬI EMAIL HỆ THỐNG] ==========");
        _logger.LogInformation("Gửi đến: {ToEmail}", toEmail);
        _logger.LogInformation("Tiêu đề: {Subject}", subject);
        _logger.LogInformation("Nội dung: {HtmlMessage}", htmlMessage);
        _logger.LogInformation("==========================================");

        return Task.CompletedTask;
    }
}

