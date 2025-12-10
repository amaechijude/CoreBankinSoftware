using Microsoft.Extensions.Options;
using Notification.IOptions;

namespace Notification.Workers;

public sealed class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    : IEmailService
{
    private readonly EmailOptions _options = options.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public Task SendEmailAsync(EmailRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SendToMultipleAsync(
        List<string> recipients,
        EmailRequest request,
        CancellationToken ct
    )
    {
        throw new NotImplementedException();
    }
}

public record EmailRequest(string Subject, string TargetEmailAddress, string Body);

internal sealed class FluentEmailExceptions(string message) : Exception(message);
