using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace NotificationWorkerService.Email;

public sealed class MailKitSettings
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
    public int TimeoutSeconds { get; set; } = 30;

    public const string Section = "MailKitSettings";
    public const string ResiliencePipelineKey = "MailKitResiliencePipeline";
}

public sealed class CustomMinLengthValidator(int minLength) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return new ValidationResult(ErrorMessage ?? "Value cannot be null");

        string? stringValue = value.ToString();

        if (string.IsNullOrWhiteSpace(stringValue))
            return new ValidationResult(ErrorMessage ?? "Value cannot be null");

        return stringValue.Length >= minLength
            ? ValidationResult.Success
            : new ValidationResult(
                ErrorMessage ?? $"Value must be at least {minLength} characters long"
            );
    }
}

[OptionsValidator]
public partial class EmailOptionsValidator : IValidateOptions<MailKitSettings> { };

internal sealed class EmailOptionsException(string message) : Exception(message);
