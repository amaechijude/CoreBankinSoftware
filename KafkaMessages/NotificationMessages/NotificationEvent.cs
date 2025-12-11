using KafkaMessages.AccountMessages;

namespace KafkaMessages.NotificationMessages;

public class NotificationEvent<T>(T value, bool success)
{
    public string NotificationId { get; } = Guid.CreateVersion7().ToString();
    public T Payload { get; } = value;
    public bool Success { get; } = success;
    public EventType EventType { get; }
}
