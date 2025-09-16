using System.ComponentModel.DataAnnotations.Schema;
using CustomerAPI.Entities.Enums;

namespace CustomerAPI.Entities
{
    public class Account : BaseEntity
    {
        public Guid CustomerId { get; private set; }
        public UserProfile UserProfile { get; private set; } = null!;
        public string AccountNumber { get; private set; } = string.Empty;
        public string BVN { get; private set; } = string.Empty;
        public AccountTier AccountTier { get; private set; }
        [Column(TypeName = "decimal(30,2)")]
        public decimal Balance { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public bool IsActive { get; private set; } = false;
        public bool IsOnPostNoDebit { get; private set; } = false;


        public static Account CreateNewAccount(UserProfile user)
        {
            return new Account
            {
                CustomerId = user.Id,
                BVN = user.BVN,
                AccountTier = AccountTier.Tier1,
                Balance = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
        }
    }

}
