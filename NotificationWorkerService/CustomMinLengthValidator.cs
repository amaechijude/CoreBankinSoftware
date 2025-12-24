using System.ComponentModel.DataAnnotations;

namespace NotificationWorkerService;

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
