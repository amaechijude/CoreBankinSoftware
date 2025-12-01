using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionStatusLog
{
    public Guid Id { get; init; }
    public Guid TransactionId { get; init; }
    public TransactionData TransactionData { get; init; } = null!;
    public TransactionStatus? PreviousStatus { get; init; }
    public TransactionStatus CurrentStatus { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    public static TransactionStatusLog Create(
        TransactionData transactionData,
        TransactionStatus currentStatus,
        string description
        )
    {
        return new TransactionStatusLog
        {
            Id = Guid.CreateVersion7(),
            TransactionId = transactionData.Id,
            TransactionData = transactionData,
            PreviousStatus = transactionData.TransactionStatus,
            CurrentStatus = currentStatus,
            Description = description,
        };
    }
}
