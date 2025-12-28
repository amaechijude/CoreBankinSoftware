using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Notification.Email;

public sealed record MailKitSettings
{
    [MinLength(7)]
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    [MinLength(5)]
    public string Username { get; set; } = string.Empty;

    [MinLength(7)]
    public string Password { get; set; } = string.Empty;

    [MinLength(5), EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [MinLength(7)]
    public string FromName { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public int TimeoutMilliSeconds { get; set; } = 30;

    public const string Section = "MailKitSettings";
    public const string ResiliencePipelineKey = "MailKitResiliencePipeline";
}
