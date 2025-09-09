using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountGrpcService.Entities
{
    public class Account
    {
        [Key]
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string AccountNumber { get; private set; } = string.Empty;
        public string BVN { get; private set; } = string.Empty;
        public AccountType AccountType { get; private set; }
        [Column(TypeName = "decimal(30,2)")]
        public decimal Balance { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public bool IsActive { get; private set; } = false;
        public bool IsOnPostNoDebit { get; private set; } = false;


        public static Account CreateNewAccount(Guid customerId, string bvn)
        {
            return new Account
            {
                Id = Guid.CreateVersion7(),
                CustomerId = customerId,
                BVN = bvn,
                AccountType = AccountType.Savings,
                Balance = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
        }
    }

    public enum AccountType
    {
        Savings,
        Checking,
        Credit
    }
}
