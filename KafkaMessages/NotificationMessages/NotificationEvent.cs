namespace KafkaMessages.NotificationMessages;


public enum NotificationChannel
{
    Email,
    SMS,
    PushNotification
}

public enum NotificationEventType
{
    AccountActivity,
    Promotional,
    SecurityAlert
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum DeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Bounced
}

public record NotificationMessage
{
    public required string NotificationId { get; init; }
    public required string UserId { get; init; }
    public required string Recipient { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required ICollection<NotificationEventType> EventType { get; set; } = new HashSet<NotificationEventType>();
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? TemplateId { get; init; }
    public Dictionary<string, object>? TemplateData { get; init; }
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public DeliveryStatus DeliveryStatus { get; init; } = DeliveryStatus.Pending;
    public int RetryCount { get; init; } = 0;
    public bool IsRead { get; set; } = false;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public string? FailureReason { get; set; }
}