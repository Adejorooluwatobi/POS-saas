using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Communication;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly string _templatePath;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
        _templatePath = Path.Combine(AppContext.BaseDirectory, "Communication", "Templates", "Email");
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var host = _config["Email:SmtpHost"];
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:SmtpUser"];
            var pass = _config["Email:SmtpPass"];
            var from = _config["Email:FromEmail"];
            var fromName = _config["Email:FromName"];

            if (string.IsNullOrEmpty(user))
            {
                _logger.LogWarning("SMTP User is not configured. Skipping email to {To}", to);
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string subject, string templateName, object placeholders, CancellationToken cancellationToken = default)
    {
        var baseTemplate = await File.ReadAllTextAsync(Path.Combine(_templatePath, "base-template.html"), cancellationToken);
        var contentTemplate = await File.ReadAllTextAsync(Path.Combine(_templatePath, $"{templateName}.html"), cancellationToken);

        // Very simple placeholder replacement
        var body = baseTemplate;
        var content = contentTemplate;

        var props = placeholders.GetType().GetProperties();
        foreach (var prop in props)
        {
            var val = prop.GetValue(placeholders)?.ToString() ?? "";
            body = body.Replace($"{{{{{prop.Name}}}}}", val);
            content = content.Replace($"{{{{{prop.Name}}}}}", val);
        }

        body = body.Replace("{{Content}}", content);
        body = body.Replace("{{Subject}}", subject);
        body = body.Replace("{{Year}}", DateTime.Now.Year.ToString());

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}
