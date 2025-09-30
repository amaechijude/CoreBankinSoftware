using TransactionService.Entity.Enums;

namespace TransactionService.Entity
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public string RefrenceNumber { get; private set; } = string.Empty;
        public Decimal Amount { get; private set; }
        public string? Narration { get; private set; } = string.Empty;
        public DateTimeOffset TimeStamp { get; private set; } = DateTimeOffset.UtcNow;

        public string SourceAccount { get; private set; } = string.Empty;
        public BeneficiaryAccount? Beneficiary { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionChannel Channel { get; private set; }
        public Status Status { get; private set; } = Status.PENDING;

    }
}
