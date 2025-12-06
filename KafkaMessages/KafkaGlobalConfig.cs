using Confluent.Kafka;
using Microsoft.Extensions.Logging;

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

public static class Broker
{
    public static async Task WaitForKafkaAsync<T>(string kafkaServer, ILogger<T> logger, CancellationToken token)
    {
        var backoff = 2;
        var adminConfig = new AdminClientConfig { BootstrapServers = kafkaServer };
        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        while (!token.IsCancellationRequested)
        {
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(2));
                return; // Success! Exit the wait loop.
            }
            catch (KafkaException ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Kafka unavailable. Retrying in {backoff}s... {Error}", backoff, ex.Error.Reason);
            }

            // Wait before retrying to avoid flooding logs
            await Task.Delay(TimeSpan.FromSeconds(backoff), token);
            backoff *= 2;
        }
    }
}