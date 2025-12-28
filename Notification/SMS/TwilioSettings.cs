using System.ComponentModel.DataAnnotations;

namespace Notification.SMS;

public sealed record TwilioSettings
{
    [MinLength(7)]
    public string AccountSid { get; init; } = string.Empty;

    [MinLength(7)]
    public string AuthToken { get; init; } = string.Empty;

    [MinLength(7)]
    public string FromNumber { get; init; } = string.Empty;
    public const string Section = "TwilioSettings";
    public const string ResiliencePipelineKey = "TwilioResiliencePipeline";
}
