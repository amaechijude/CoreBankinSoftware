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
    private const int BatchSize = 100;
    private const int InitialDelayMs = 4000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. BLOCK UNTIL KAFKA IS READY
        await Broker.WaitForKafkaAsync(KafkaGlobalConfig.BootstrapServers, logger, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("TransactionEventPublisher shutting down");
                break;
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(
                        ex,
                        "Unexpected error in TransactionEventPublisher at {Timestamp}",
                        DateTimeOffset.UtcNow
                    );
            }

            if (!stoppingToken.IsCancellationRequested)
                await Task.Delay(GetDelayWithJitter(InitialDelayMs), stoppingToken);
        }
    }

    private async Task PublishAsync(CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var skip = 0;
        var hasMore = true;

        while (hasMore && !ct.IsCancellationRequested)
        {
            var messages = await dbContext
                .OutboxMessages.Where(m => m.Status == OutboxStatus.Pending)
                .OrderBy(m => m.CreatedAt)
                .Skip(skip)
                .Take(BatchSize)
                .ToListAsync(ct);

            hasMore = messages.Count == BatchSize;

            if (messages.Count == 0)
                break;

            List<OutboxMessage> messagesToUpdate = [];

            foreach (var outboxMsg in messages)
            {
                try
                {
                    var @event = CreateEvent(outboxMsg);
                    var kafkaMessage = new Message<string, string>
                    {
                        Key = outboxMsg.TransactionId.ToString(),
                        Value = CustomMessageSerializer.Serialize(@event),
                    };

                    var deliveryReport = await kafkaProducer.ProduceAsync(_topic, kafkaMessage, ct);

                    if (deliveryReport.Status == PersistenceStatus.Persisted)
                    {
                        outboxMsg.Status = OutboxStatus.Published;
                        outboxMsg.PublishedAt = DateTimeOffset.UtcNow;
                        messagesToUpdate.Add(outboxMsg);
                    }
                    else
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning(
                                "Message {TransactionId} failed to persist. Status: {Status}",
                                outboxMsg.TransactionId,
                                deliveryReport.Status
                            );
                    }
                }
                catch (KafkaException kafkaEx)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                        logger.LogError(
                            kafkaEx,
                            "Kafka error publishing message {TransactionId}. Code: {Code}",
                            outboxMsg.TransactionId,
                            kafkaEx.Error.Code
                        );
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                        logger.LogError(
                            ex,
                            "Error processing outbox message {TransactionId}",
                            outboxMsg.TransactionId
                        );
                }
            }

            // Batch update only successfully published messages
            if (messagesToUpdate.Count > 0)
            {
                dbContext.OutboxMessages.UpdateRange(messagesToUpdate);
                await dbContext.SaveChangesAsync(ct);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation(
                        "Published {Count} messages to Kafka",
                        messagesToUpdate.Count
                    );
            }

            skip += BatchSize;
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
            EventType = MapTransactionType(message.TransactionType),

            SendersAccountName = "",
            SendersAccountNumber = "",
            DestinationAccountName = "",
        };
    }

    private static EventType MapTransactionType(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Credit => EventType.Credit,
            TransactionType.Debit => EventType.Debit,
            TransactionType.Transfer => EventType.Transfer,
            _ => EventType.Utility,
        };
    }

    private static TimeSpan GetDelayWithJitter(int baseDelayMs)
    {
        var jitterMs = Random.Shared.Next(0, 1000);
        return TimeSpan.FromMilliseconds(baseDelayMs + jitterMs);
    }
}
