using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Registry;

namespace NotificationWorkerService.Email;

internal sealed class EmailService(
    ILogger<EmailService> logger,
    IOptions<MailKitSettings> options,
    ResiliencePipelineProvider<string> pipelineProvider
)
{
    private readonly ILogger<EmailService> _logger = logger;
    private readonly MailKitSettings _options = options.Value;
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline(
        MailKitSettings.ResiliencePipelineKey
    );

    public async Task<bool> SendEmailAsync(EmailRequest request, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(request.FullName, request.TargetEmailAddress));
        message.Subject = request.Subject;
        message.Body = new TextPart("html") { Text = request.Body };

        try
        {
            await SendMimeMessage(message, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return false;
        }
    }

    private async Task SendMimeMessage(MimeMessage mimeMessage, CancellationToken cancellationToken)
    {
        await _pipeline.ExecuteAsync(
            async ct =>
            {
                using var client = new SmtpClient();
                client.Timeout = _options.TimeoutSeconds * 1000;

                var secureSocketOptions = _options.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(
                    host: _options.SmtpHost,
                    port: _options.SmtpPort,
                    options: secureSocketOptions,
                    cancellationToken: ct
                );

                await client.AuthenticateAsync(
                    userName: _options.Username,
                    password: _options.Password,
                    cancellationToken: ct
                );

                await client.SendAsync(message: mimeMessage, cancellationToken: ct);

                await client.DisconnectAsync(quit: true, cancellationToken: ct);
            },
            cancellationToken
        );
    }
}

internal record EmailRequest(
    string Subject,
    string TargetEmailAddress,
    string FullName,
    string Body
);
