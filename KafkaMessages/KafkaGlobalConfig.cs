namespace KafkaMessages;

public static class KafkaGlobalConfig
{
    public const string BootstrapServers = "localhost:9092";
    public const string TransactionToAccountTopic = "transaction-account";
    public const string TransactionToAccountGroupId = "transaction-account-group-id";
    public const string NotificationTopic = "notification";
    public const string AccountToTransactionTopic = "account-transaction";
    public const string LoggingTopic = "logging";
}
