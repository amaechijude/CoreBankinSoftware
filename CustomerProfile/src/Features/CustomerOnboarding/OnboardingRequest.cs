using System.ComponentModel.DataAnnotations;
using FluentValidation;
using src.Domain.Entities;

namespace src.Features.CustomerOnboarding
{
    public class OnboardingRequest
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits.")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;

        public OnboardingRequest()
        {
            PhoneNumber = NormalizePhoneNumber(PhoneNumber);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "";
            phoneNumber = phoneNumber.Trim().Replace("-", "").Replace(" ", "");

            return "+234" + phoneNumber[1..];
        }
    }

    public class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
    {
        public OnboardingRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(\+234|0)?[789]\d{9}$")
                .WithMessage("Invalid phone number format");
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
