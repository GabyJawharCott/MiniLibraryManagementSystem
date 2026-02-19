using System.Net;
using System.Net.Mail;

namespace MiniLibraryManagementSystem.Services;

/// <summary>Email notification when a book is returned. Configure SMTP in appsettings (SmtpSettings).</summary>
public class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _config;

    public EmailNotificationService(ILogger<EmailNotificationService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task NotifyBookReturnedAsync(string userEmail, string? userName, string bookTitle, DateTime returnedAt)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogWarning("No email for borrower; cannot send return notification for book '{Title}'", bookTitle);
            return;
        }

        var smtp = _config.GetSection("SmtpSettings");
        var host = smtp["Host"];
        var port = smtp.GetValue<int?>("Port");
        var from = smtp["From"];
        var user = smtp["User"];
        var password = smtp["Password"];
        var enableSsl = smtp.GetValue<bool>("EnableSsl");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(from))
        {
            _logger.LogWarning("SmtpSettings not configured; skipping email to {Email} for book '{Title}'", userEmail, bookTitle);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port ?? 587);
            client.EnableSsl = enableSsl;
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                client.Credentials = new NetworkCredential(user, password);

            var displayName = string.IsNullOrEmpty(userName) ? userEmail : userName;
            var subject = "Your book has been returned – Library";
            var body = $"""
                Hello {displayName},

                The following book has been marked as returned:

                "{bookTitle}"
                Returned at: {returnedAt:yyyy-MM-dd HH:mm} UTC

                Thank you for using the library.
                """;

            var message = new MailMessage(from!, userEmail, subject, body);
            await client.SendMailAsync(message);
            _logger.LogInformation("Notified {Email} that book '{Title}' was returned.", userEmail, bookTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send return notification to {Email} for book '{Title}'", userEmail, bookTitle);
            throw;
        }
    }
}
