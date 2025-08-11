using FluentValidation;

namespace src.Features.Customer.Onboarding
{
    public record OnboardingRequest(string PhoneNumber);
    public record OnboardingResponse(string Phone, string Code, string ExpiresIn);

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

}
