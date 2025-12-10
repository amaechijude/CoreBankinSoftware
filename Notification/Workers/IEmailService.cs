namespace Notification.Workers;

public interface IEmailService
{
    Task SendEmailAsync(EmailRequest request, CancellationToken ct);
    Task SendToMultipleAsync(List<string> recipients, EmailRequest request, CancellationToken ct);
}
