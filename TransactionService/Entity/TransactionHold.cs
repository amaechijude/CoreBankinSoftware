using TransactionService.Entity.Enums;

namespace TransactionService.Entity
{
    public class TransactionHold
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string HoldReference { get; private set; } = string.Empty;
        public string TransactionReference { get; private set; } = string.Empty;
        public string AccountNumber { get; private set; } = string.Empty;
        public decimal HoldAmount { get; private set; }
        public HoldType HoldType { get; private set; }
        public HoldStatus Status { get; private set; }
        public DateTimeOffset PlacedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReleasedAt { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public string PlacedByService { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    }
}
