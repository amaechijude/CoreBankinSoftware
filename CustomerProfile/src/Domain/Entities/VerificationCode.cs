using System.ComponentModel.DataAnnotations;
using src.Shared.Global;

namespace src.Domain.Entities
{
    public class VerificationCode
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; } = false;
        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
        public VerificationCode()  { }
        public VerificationCode(string phoneNumber)
        {
            UserPhoneNumber = phoneNumber;
            Code = GlobalConstansts.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        }
        public void MarkAsUsed()
        {
            IsUsed = true;
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        }
        public void UpdateCode()
        {
            Code = GlobalConstansts.GenerateVerificationCode();
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
            IsUsed = false;
        }

        public bool IsValid()
        {
            return !IsUsed && !IsExpired;
        }

    }
}
