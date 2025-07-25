using System.ComponentModel.DataAnnotations;

namespace src.Features.CustomerOnboarding
{
    public class CustomerOnboardingCommand
    {
        [Phone]
        [Required(ErrorMessage = "Phone number is required.")]
        [NigerianPhoneNumberValidator(ErrorMessage = "Invalid Nigerian phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
