using System.ComponentModel.DataAnnotations;
using src.Shared.Global;

namespace src.Shared.Domain.Entities
{
    public class VerificationCode(string phnoneNumber)
    {
        [Key]
        public string Code { get; private set; } = GlobalConstansts.GenerateVerificationCode();
        public string UserPhoneNumber { get; private set; } = phnoneNumber;
        public DateTimeOffset ExpiresAt { get; private set; } = DateTimeOffset.UtcNow.AddMinutes(15);
        public bool IsUsed { get; private set; } = false;
        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

        public void MarkAsUsed()
        {
            IsUsed = true;
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        }

        public bool IsValid()
        {
            return !IsUsed && !IsExpired;
        }

    }
}
