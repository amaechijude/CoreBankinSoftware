using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace CustomerProfile.DTO;

public sealed record OnboardingRequest(string PhoneNumber, string Email);

public sealed record OnboardingResponse(string Token, DateTime? ExpiresIn);

public sealed class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
{
    public OnboardingRequestValidator()
    {
        RuleFor(x => x.PhoneNumber.Trim())
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Length(11)
            .WithMessage("Phone number lenght Must be 11")
            .Matches(@"^(\+234|0)?[789]\d{9}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Email.Trim()).EmailAddress().WithMessage("Invalid Email Address");
    }
}

public sealed record VerifyOtpResponse(string Message);

public sealed class OtpVerifyRequestBody
{
    [Required, MinLength(6)]
    public string OtpCode { get; set; } = string.Empty;
}
