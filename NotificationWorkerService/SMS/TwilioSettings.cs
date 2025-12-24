using Microsoft.Extensions.Options;

namespace NotificationWorkerService.SMS;

public sealed record TwilioSettings
{
    [CustomMinLengthValidator(7)]
    public string AccountSid { get; init; } = string.Empty;

    [CustomMinLengthValidator(7)]
    public string AuthToken { get; init; } = string.Empty;

    [CustomMinLengthValidator(7)]
    public string FromNumber { get; init; } = string.Empty;
    public const string Section = "TwilioSettings";
    public const string ResiliencePipelineKey = "TwilioResiliencePipeline";
}

[OptionsValidator]
public partial class TwilioOptionsValidator : IValidateOptions<TwilioSettings> { };
