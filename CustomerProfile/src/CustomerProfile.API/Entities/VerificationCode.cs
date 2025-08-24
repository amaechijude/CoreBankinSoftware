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
        public bool CanSetProfile { get; private set; } = false;
        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

        private static readonly int _expiryDurationInMinutes = 10;
        public string ExpiryDuration => $"{_expiryDurationInMinutes} minutes";

        // Factory method to create 
        public static VerificationCode CreateNew(string phoneNumber, string? email)
        {
            return new VerificationCode
            {
                UserPhoneNumber = phoneNumber,
                UserEmail = email ?? string.Empty,
                Code = GlobalUtils.GenerateVerificationCode(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes),
            };

        }
        public void MarkIsUsedAndCanSetProfile()
        {
            IsUsed = true;
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            CanSetProfile = true;
        }
        public void UpdateCode()
        {
            Code = GlobalUtils.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes);
            IsUsed = false;
        }

    }
}
