using System.ComponentModel.DataAnnotations;

namespace src.Features.CustomerOnboarding
{
    public class CustomerOnboardingCommand
    {
        [Phone]
        [Required(ErrorMessage = "Phone number is required.")]
        [MinLength(4), MaxLength(16)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
