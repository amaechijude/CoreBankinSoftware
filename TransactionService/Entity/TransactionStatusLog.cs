using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionStatusLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TransactionReference { get; private set; } = string.Empty;
    public TransactionStatus? PreviousStatus { get; private set; }
    public TransactionStatus NewStatus { get; private set; }
    public string ChangeReason { get; private set; } = string.Empty;
    public string? Metadata { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
