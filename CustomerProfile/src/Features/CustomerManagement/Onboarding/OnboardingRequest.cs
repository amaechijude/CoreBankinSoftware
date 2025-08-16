using FluentValidation;

namespace UserProfile.API.Features.CustomerManagement.Onboarding
{
    public record OnboardingRequest(string PhoneNumber);
    public record OnboardingResponse(string Token, string ExpiresIn);

    public class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
    {
        public OnboardingRequestValidator()
        {
            RuleFor(x => x.PhoneNumber.Trim())
                .NotEmpty().WithMessage("Phone number is required.")
                .Length(11).WithMessage("Phone number lenght Must be 11")
                .Matches(@"^(\+234|0)?[789]\d{9}$").WithMessage("Invalid phone number format");
        }
    }

    public record VerifyOtpResponse(string Message);

    public record OtpVerifyRequest(string Code);
}
