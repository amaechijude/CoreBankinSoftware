using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace NotificationWorkerService.Email;

public sealed record MailKitSettings
{
    [CustomMinLengthValidator(7)]
    public string SmtpHost { get; set; } = string.Empty;

    [CustomMinLengthValidator(2)]
    public int SmtpPort { get; set; }

    [CustomMinLengthValidator(5)]
    public string Username { get; set; } = string.Empty;

    [CustomMinLengthValidator(7)]
    public string Password { get; set; } = string.Empty;

    [CustomMinLengthValidator(5), EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [CustomMinLengthValidator(7)]
    public string FromName { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public int TimeoutMilliSeconds { get; set; } = 30;

    public const string Section = "MailKitSettings";
    public const string ResiliencePipelineKey = "MailKitResiliencePipeline";
}

[OptionsValidator]
public partial class EmailOptionsValidator : IValidateOptions<MailKitSettings> { };
