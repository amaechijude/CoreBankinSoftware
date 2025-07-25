using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace src.Features.CustomerOnboarding
{
    public class OnboardingRequest
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits.")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
    {
        public OnboardingRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Length(11).WithMessage("Phone number must be exactly 11 digits.")
                .Matches(@"^(\+234|0)?[789]\d{9}$")
                .WithMessage("Invalid phone number format");
        }
    }
}
