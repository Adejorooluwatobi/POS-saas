namespace POS.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendTemplatedEmailAsync(string to, string subject, string templateName, object placeholders, CancellationToken cancellationToken = default);
}
