using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Registry;

namespace Notification.Email;

internal sealed class EmailService(

        IHostEnvironment environment,
    ILogger<EmailService> logger,
    IOptions<MailKitSettings> options,
    ResiliencePipelineProvider<string> pipelineProvider
)
{
    private readonly IHostEnvironment _environment = environment;
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
            //voi_ = _environment.IsProduction() switch
            //{
            //    true => await SendViaMailKitAsync(message, ct),
            //    false => await SendViaPapercutAsync(message, ct),
            //};
            if (_environment.IsProduction())
            {
                await SendViaMailKitAsync(message, ct);
            } else
            {
                await SendViaPapercutAsync(message, ct);
            }
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return false;
        }
    }

    private async Task SendViaMailKitAsync(MimeMessage mimeMessage, CancellationToken cancellationToken)
    {
        await MailkitHandler.pipeline.ExecuteAsync(
            async ct =>
            {
                using var client = new SmtpClient();
                try
                {
                    client.Timeout = _options.TimeoutMilliSeconds * 1000;

                    var secureSocketOptions = _options.UseSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

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
                }
                finally
                {
                    await client.DisconnectAsync(quit: true, cancellationToken: ct);
                }
            },
            cancellationToken
        );
    }
    private static async Task SendViaPapercutAsync(MimeMessage mimeMessage, CancellationToken ct)
    {
        using var smtp = new SmtpClient();

        await smtp.ConnectAsync("localhost", 25, MailKit.Security.SecureSocketOptions.None, ct);

        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true, ct);
    }
}

internal record EmailRequest(
    string Subject,
    string TargetEmailAddress,
    string FullName,
    string Body
);
