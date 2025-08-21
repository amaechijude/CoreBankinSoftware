using System.ComponentModel.DataAnnotations;
using CustomerAPI.DTO;
using CustomerAPI.Global;

namespace CustomerAPI.Entities
{
    public class VerificationCode
    {
        [Key]
        public Guid Id { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string UserPhoneNumber { get; private set; } = string.Empty;
        public string UserEmail { get; private set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; } = false;
        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

        private static readonly int _expiryDurationInMinutes = 10;
        public string ExpiryDuration => $"{_expiryDurationInMinutes} minutes";

        // Factory method to create 
        public static VerificationCode CreateNew(OnboardingRequest request)
        {
            return new VerificationCode
            {
                UserPhoneNumber = request.PhoneNumber.Trim(),
                UserEmail = request.Email.Trim(),
                Code = GlobalUtils.GenerateVerificationCode(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes),
            };

        }
        public void MarkAsUsed()
        {
            IsUsed = true;
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        }
        public void UpdateCode()
        {
            Code = GlobalUtils.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes);
            IsUsed = false;
        }

    }
}
