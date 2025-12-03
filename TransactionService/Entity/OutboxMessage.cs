using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class OutboxMessage
{
    public Guid Id { get; private init; }
    public Guid CustomerId { get; private init; }
    public string? DestinationAccountNumber { get; private init; }
    public string? DestinationBankName { get; private init; }
    public Guid TransactionId { get; private init; }
    public string TransactionReference { get; private init; } = string.Empty;
    public string SessionId { get; private init; } = string.Empty;
    public decimal Amount { get; private init; }
    public decimal TransactionFee { get; private init; }
    public TransactionType TransactionType { get; private init; }
    public string TransactionStatus { get; private set; } = string.Empty;
    public OutboxStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? PublishedAt { get; set; }


    public static OutboxMessage Create(TransactionData transactionData)
    {
        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            TransactionId = transactionData.Id,
            CustomerId = transactionData.CustomerId,
            TransactionReference = transactionData.TransactionReference,
            SessionId = transactionData.SessionId,
            DestinationAccountNumber = transactionData.DestinationAccountNumber,
            DestinationBankName = transactionData.DestinationBankName,
            Amount = transactionData.Amount,
            TransactionFee = transactionData.TransactionFee,
            TransactionType = transactionData.TransactionType,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OutboxStatus.Pending,
            TransactionStatus = transactionData.TransactionStatus.ToString()
        };
    }
}

public enum OutboxStatus
{
    Pending,
    Published,
    Failed
}