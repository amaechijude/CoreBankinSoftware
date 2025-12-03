
using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Entity;
using TransactionService.Entity.Enums;

namespace TransactionService.Services;

public class TransactionEventPublisher(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TransactionEventPublisher> logger,
    IProducer<string, string> kafkaProducer
    ) : BackgroundService
{
    private static readonly string _topic = KafkaGlobalConfig.TransactionToAccountTopic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (Broker.IsKafkaBrokerAvailable())
            {
                try
                {
                    await PublishAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                        logger.LogError(ex, "Error trying to publish to kafka topic {}", DateTimeOffset.UtcNow);
                }
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Critical))
                    logger.LogCritical("Kafka producer service down {}", DateTimeOffset.UtcNow);
            }
            if (stoppingToken.IsCancellationRequested)
                break;
            await Task.Delay(Delay4SecondsWithJitter(), stoppingToken);
        }
    }

    private async Task PublishAsync(CancellationToken ct)
    {
        await using (var scope = serviceScopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            var messages = await dbContext.OutboxMessages
                    .Where(m => m.Status == OutboxStatus.Pending)
                    .ToListAsync(ct);
            if (messages.Count != 0)
            {
                foreach (var m in messages)
                {
                    Message<string, string> message = new()
                    {
                        Key = m.TransactionId.ToString(),
                        Value = CustomMessageSerializer
                            .Serialize(CreateEvent(m))
                    };

                    await kafkaProducer.ProduceAsync(topic: _topic, message: message, cancellationToken: ct);

                    m.Status = OutboxStatus.Published;
                    m.PublishedAt = DateTimeOffset.UtcNow;
                }
                await dbContext.SaveChangesAsync(ct);
            }
        }
    }

    private static TransactionAccountEvent CreateEvent(OutboxMessage message)
    {
        return new TransactionAccountEvent
        {
            CustomerId = message.CustomerId,
            TransactionId = message.TransactionId,
            TransactionReference = message.TransactionReference,
            SessionId = message.SessionId,
            DestinationAccountNumber = message.DestinationAccountNumber ?? string.Empty,
            DestinationBankName = message.DestinationBankName ?? string.Empty,
            Amount = message.Amount,
            TransactionFee = message.TransactionFee,
            Timestamp = message.CreatedAt,
            EventType = message.TransactionType == TransactionType.Credit
                ? EventType.Credit
                : message.TransactionType == TransactionType.Debit
                    ? EventType.Debit
                    : message.TransactionType == TransactionType.Transfer
                        ? EventType.Transfer
                        : EventType.Utility
        };
    }

    private static TimeSpan Delay4SecondsWithJitter()
    {
        var jitterMs = (int)(Random.Shared.NextDouble() * 1000.0);
        return TimeSpan.FromMilliseconds(4000 + jitterMs);
    }
}

// 

public static class Broker
{
    private static bool? _isKafkaBrokerAvailable;
    private static DateTime _lastCheckTime;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(3.9);

    public static bool IsKafkaBrokerAvailable()
    {
        // Check if the cached result is still valid
        if (_isKafkaBrokerAvailable.HasValue && (DateTime.UtcNow - _lastCheckTime) < CacheDuration)
        {
            _lastCheckTime = DateTime.UtcNow;
            return _isKafkaBrokerAvailable.Value;
        }

        try
        {
            var adminConfig = new AdminClientConfig { BootstrapServers = KafkaGlobalConfig.BootstrapServers };
            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            _isKafkaBrokerAvailable = metadata.Brokers.Count > 0;
        }
        catch
        {
            _isKafkaBrokerAvailable = false;
        }

        // Update the last check time
        _lastCheckTime = DateTime.UtcNow;
        return _isKafkaBrokerAvailable.Value;
    }
}