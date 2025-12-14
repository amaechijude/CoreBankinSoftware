namespace KafkaMessages.AccountMessages;

public record TransactionAccountEvent
{
    public required Guid CustomerId { get; init; }
    public required string SendersAccountName { get; init; }
    public required string SendersAccountNumber { get; init; }
    public required string DestinationAccountName { get; init; }
    public required Guid TransactionId { get; init; }
    public required string TransactionReference { get; init; }
    public required string SessionId { get; init; }
    public required string DestinationAccountNumber { get; init; }
    public required string DestinationBankName { get; init; }
    public required decimal Amount { get; init; }
    public required decimal TransactionFee { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public EventType EventType { get; init; }
}

public enum EventType
{
    Credit,
    Debit,
    Transfer,
    Utility,
}

public class Profile { }
