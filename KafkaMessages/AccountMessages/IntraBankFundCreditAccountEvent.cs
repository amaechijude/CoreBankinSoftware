
using System.Transactions;

namespace KafkaMessages.AccountMessages;

public record AccountDebitEvent
{
    public required Guid TransactionId { get; init; }
    public required string TransactionReference { get; init; }
    public required string SessionId { get; init; }
    public required string AccountNumber { get; init; }
    public required decimal Amount { get; init; }
    public required decimal TransactionFee { get; init; }
    public EventType EventType { get; } = EventType.Debit;
    public required DateTimeOffset Timestamp { get; init; }
}

public record AccountCreditEvent
{
    public required Guid TransactionId { get; init; }
    public required string TransactionReference { get; init; }
    public required string SessionId { get; init; }
    public required string AccountNumber { get; init; }
    public required decimal Amount { get; init; }
    public required decimal TransactionFee { get; init; }
    public EventType EventType { get; } = EventType.Credit;
    public required DateTimeOffset Timestamp { get; init; }
}

public record AccountUtilityEvent
{
    public required Guid TransactionId { get; init; }
    public required string TransactionReference { get; init; }
    public required string SessionId { get; init; }
    public required string AccountNumber { get; init; }
    public required decimal Amount { get; init; }
    public required decimal TransactionFee { get; init; }
    public EventType EventType { get; } = EventType.Utility;
    public required DateTimeOffset Timestamp { get; init; }
}

public enum EventType
{
    Credit,
    Debit,
    Utility
}