using TransactionService.Entity.Enums;

namespace TransactionService.Entity
{
    public class TransactionNotification
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string TransactionReference { get; private set; } = string.Empty;
        public RecipientType RecipientType { get; private set; }
        public NotificationType NotificationType { get; private set; }
        public string RecipientContact { get; private set; } = string.Empty;
        public string MessageContent { get; private set; } = string.Empty;
        public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
        public string? ProviderReference { get; private set; }
        public string? ProviderResponse { get; private set; }
        public int RetryCount { get; private set; } = 0;
        public DateTimeOffset? SentAt { get; private set; }
        public DateTimeOffset? DeliveredAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    }
}
