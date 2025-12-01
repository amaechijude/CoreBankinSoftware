using KafkaMessages;

namespace TransactionService.Entity;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public Guid TransactionId { get; init; }
    public Guid CorrelationId { get; init; }
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The JSON-serialized payload of the event.
    /// </summary>
    public string Payload { get; init; } = string.Empty;
    public OutboxStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? ProcessedAt { get; set; }


    public static OutboxMessage Create<T>(Guid transactionId, Guid correlationId, T payload)
    {
        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            TransactionId = transactionId,
            CorrelationId = correlationId,
            EventType = nameof(T),
            Payload = CustomMessageSerializer.Serialize(payload),
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OutboxStatus.Pending
        };
    }
}

public enum OutboxStatus
{
    Pending,
    Published,
    Failed
}