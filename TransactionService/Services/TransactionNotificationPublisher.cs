using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Entity;
using TransactionService.Entity.Enums;

namespace TransactionService.Services;

public class TransactionNotificationPublisher(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TransactionNotificationPublisher> logger,
    IProducer<string, string> kafkaProducer
) : BackgroundService
{
    private static readonly string _topic = KafkaGlobalConfig.NotificationTopic;
    private const int InitialDelayMs = 4000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. BLOCK UNTIL KAFKA IS READY
        await Broker.WaitForKafkaAsync(KafkaGlobalConfig.BootstrapServers, logger, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(GetDelayWithJitter(InitialDelayMs), stoppingToken);
        }
    }

    private async Task PublishAsync(CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
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
