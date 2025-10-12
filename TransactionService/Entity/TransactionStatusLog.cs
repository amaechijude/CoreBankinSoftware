using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionStatusLog
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public TransactionStatus? PreviousStatus { get; set; }
    public TransactionStatus NewStatus { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
