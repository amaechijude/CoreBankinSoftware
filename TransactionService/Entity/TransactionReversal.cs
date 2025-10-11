using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionReversal
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ReversalReference { get; private set; } = string.Empty;
    public string OriginalTransactionReference { get; private set; } = string.Empty;
    public string ReversalTransactionReference { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public ReversalStatus Status { get; private set; } = ReversalStatus.Pending;
    public string InitiatedBy { get; private set; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; private set; }
}
