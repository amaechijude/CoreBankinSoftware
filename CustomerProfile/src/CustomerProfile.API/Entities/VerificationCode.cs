using System.ComponentModel.DataAnnotations;
using CustomerAPI.Global;

namespace CustomerAPI.Entities
{
    public class VerificationCode
    {
        [Key]
        public Guid Id { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string UserPhoneNumber { get; private set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; } = false;
        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

        private static readonly int _expiryDurationInMinutes = 10;
        public string ExpiryDuration => $"{_expiryDurationInMinutes} minutes";

        public VerificationCode()  { }
        public VerificationCode(string phoneNumber)
        {
            UserPhoneNumber = phoneNumber;
            Code = GlobalConstansts.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes);
        }
        public void MarkAsUsed()
        {
            IsUsed = true;
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        }
        public void UpdateCode()
        {
            Code = GlobalConstansts.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes);
            IsUsed = false;
        }

    }
}
