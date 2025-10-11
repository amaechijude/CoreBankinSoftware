using TransactionService.Entity.Enums;

namespace TransactionService.Entity
{
    public class TransactionDispute
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string DisputeReference { get; private set; } = string.Empty;
        public string TransactionReference { get; private set; } = string.Empty;
        public DisputeType DisputeType { get; private set; }
        public string RaisedByAccount { get; private set; } = string.Empty;
        public string DisputeDescription { get; private set; } = string.Empty;
        public DisputeStatus Status { get; private set; } = DisputeStatus.Open;
        public string? Resolution { get; private set; }
        public string? ResolvedByUserId { get; private set; }
        public DateTimeOffset RaisedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ResolvedAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; private set; }
    }
}
