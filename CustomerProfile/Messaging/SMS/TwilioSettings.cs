using System.ComponentModel.DataAnnotations;

namespace CustomerProfile.Messaging.SMS
{
    public sealed class TwilioSettings
    {
        [Required(ErrorMessage = "Account SID is required.")]
        [MinLength(20, ErrorMessage = "Account SID must be at least 20 characters long.")]
        public string AccountSid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Auth Token is required.")]
        [MinLength(22, ErrorMessage = "Auth Token must be at least 22 characters long.")]
        public string AuthToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "From Phone Number is required.")]
        [MinLength(10, ErrorMessage = "From Phone Number must be at least 10 characters long.")]
        public string FromPhoneNumber { get; set; } = string.Empty;
    }
}
