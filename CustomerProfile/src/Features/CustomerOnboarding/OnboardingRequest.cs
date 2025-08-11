using FluentValidation;
using src.Domain.Entities;

namespace src.Features.CustomerOnboarding
{
    public record OnboardingRequest(string PhoneNumber);
    
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

    public class OnboardingResponse(VerificationCode code)
    {
        public string Phone { get; } = code.UserPhoneNumber;
        public string VerificationCode { get; } = code.Code;
        public string Expiresin { get; } = code.ExpiryDuration;
    }

    public class NinRequest
    {
        public IFormFile? Image { get; set; }
        public string Url { get; set; } = "https://x.com/amaechi_1/photo";
    }

    public record NINResponse(bool IsValid, float[]? Embediings);
}
