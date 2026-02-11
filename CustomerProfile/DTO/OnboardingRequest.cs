using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace CustomerProfile.DTO;

public sealed record OnboardingRequest(string PhoneNumber, string Email);

public sealed record OnboardingResponse(string Token, DateTime? ExpiresIn);

public class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
{
    public OnboardingRequestValidator()
    {
        RuleFor(x => x.PhoneNumber.Trim())
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Length(11)
            .WithMessage("Phone number lenght Must be 11")
            .Must(IsAllAsciiDigit)
            .WithMessage("Phone number must contain only digits.");
    }

    private bool IsAllAsciiDigit(string phoneNumber) =>
        !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.All(char.IsAsciiDigit);
}

public sealed record VerifyOtpResponse(Guid SessionId, string Message);

public sealed record OtpVerifyRequestBody([Required, MinLength(6), MaxLength(7)] string OtpCode);

public sealed record SetSixDigitPinRequest(
    [Required, StringLength(6)] string Pin,
    [Required] string ConfirmPin
);

public class SetSixDigitPinRequestValidator : AbstractValidator<SetSixDigitPinRequest>
{
    public SetSixDigitPinRequestValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty()
            .WithMessage("Pin is required")
            .Length(6)
            .WithMessage("Pin must be 6 characters long")
            .Must(IsAllAsciiDigit)
            .WithMessage("Pin must contain only digits");

        RuleFor(x => x.ConfirmPin)
            .NotEmpty()
            .WithMessage("Confirm pin is required")
            .Equal(x => x.Pin)
            .WithMessage("Confirm pin must match pin");
    }

    private bool IsAllAsciiDigit(string pin) =>
        !string.IsNullOrWhiteSpace(pin) && pin.All(char.IsAsciiDigit);
}
