using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using System.Threading.Channels;
using TransactionService.Entity;
using TransactionService.Services;

namespace TransactionService.Workers;

public sealed class NotificationWithChannelsWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<NotificationWithChannelsWorker> logger,
    Channel<OutboxMessage> channel,
    IProducer<string, string> kafkaProducer
) : BackgroundService
{
    private static readonly string _topic = KafkaGlobalConfig.TransactionNotificationTopic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessMessagesAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(ex, "publishement faled");
                    }
                }
            }
        }
    }

    private async Task ProcessMessagesAsync(OutboxMessage message, CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var userPreference = scope.ServiceProvider.GetRequiredService<UserPreferenceService>();

        if (message.TransactionType == Entity.Enums.TransactionType.Transfer)
        {
            await PublishTransferAsync(message, userPreference, ct);
        }
        else
        {
            await PublishOthersAsync(message, userPreference, ct);
        }
    }

    private async Task PublishOthersAsync(
        OutboxMessage message,
        UserPreferenceService userPreference,
        CancellationToken ct
    )
    {
        var preference = await userPreference.GetByCustomerId(message.CustomerId, ct);
        if (preference is null)
        {
            return;
        }

        var eventType = message.TransactionType switch
        {
            Entity.Enums.TransactionType.Deposit => EventType.Deposit,
            Entity.Enums.TransactionType.Withdrawal => EventType.Withdrawal,
            Entity.Enums.TransactionType.Debit => EventType.Debit,
            Entity.Enums.TransactionType.Credit => EventType.Credit,
            _ => EventType.Utility,
        };
        var accountEvent = CreateEvent(message, preference, eventType);
        if (await ProduceMessageAsync(accountEvent, ct))
        {
            await userPreference.MarkOutboxPublished(message.TransactionId, ct);
        }
    }

    private async Task PublishTransferAsync(
        OutboxMessage message,
        UserPreferenceService userPreference,
        CancellationToken ct
    )
    {
        var preference = await userPreference.GetDetailsForTransfer(
            message.CustomerId,
            message.DestinationAccountNumber!,
            ct
        );
        if (preference.Count == 0)
        {
            return;
        }

        preference.TryGetValue(message.CustomerId.ToString(), out var sender);
        preference.TryGetValue(message.DestinationAccountNumber!, out var beneficiary);

        if (sender is null || beneficiary is null)
        {
            return;
        }

        var senderEvent = CreateEvent(message, sender, EventType.TransferDebit);
        var beneficiaryEvent = CreateEvent(message, beneficiary, EventType.TransferCredit);

        var se = ProduceMessageAsync(senderEvent, ct);
        var be = ProduceMessageAsync(beneficiaryEvent, ct);
        await Task.WhenAll(se, be);

        if (await se && await be)
        {
            await userPreference.MarkOutboxPublished(message.TransactionId, ct);
        }

        return;
    }

    private async Task<bool> ProduceMessageAsync(
        TransactionAccountEvent accountEvent,
        CancellationToken ct
    )
    {
        var messageValue = CustomMessageSerializer.Serialize(accountEvent);
        var kafkaMessage = new Message<string, string>
        {
            Key = accountEvent.TransactionId.ToString(),
            Value = messageValue,
        };

        try
        {
            var deliveryResult = await kafkaProducer.ProduceAsync(_topic, kafkaMessage, ct);

            return deliveryResult.Status == PersistenceStatus.Persisted;
        }
        catch (ProduceException<string, string> ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Failed to deliver message: {Reason}", ex.Error.Reason);
            }

            return false;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "An error occurred while producing message to Kafka.");
            }

            return false;
        }
    }

    private static TransactionAccountEvent CreateEvent(
        OutboxMessage message,
        UserNotificationPreference preference,
        EventType eventType
    )
    {
        return new TransactionAccountEvent
        {
            Email = preference.Email,
            PhoneNumber = preference.PhoneNumber,
            TransactionId = message.TransactionId,
            TransactionReference = message.TransactionReference,
            SessionId = message.SessionId,
            DestinationAccountNumber = message.DestinationAccountNumber ?? string.Empty,
            DestinationBankName = message.DestinationBankName ?? string.Empty,
            DestinationAccountName = preference.FullName,
            Amount = message.Amount,
            TransactionFee = message.TransactionFee,
            Timestamp = message.CreatedAt,
            EventType = eventType,
            SendersAccountName = preference.FullName,
            SendersBankName = message.BankName,
            SendersAccountNumber = preference.AccountNumber,
        };
    }
}


public sealed class TestProduce(IProducer<string, string> kafkaProducer, ILogger<TestProduce> logger)
{
    public async Task<bool> ProduceMessageAsync(
        TransactionAccountEvent accountEvent,
        CancellationToken ct
    )
    {
        var messageValue = CustomMessageSerializer.Serialize(accountEvent);
        var kafkaMessage = new Message<string, string>
        {
            Key = accountEvent.TransactionId.ToString(),
            Value = messageValue,
        };

        try
        {
            var deliveryResult = await kafkaProducer.ProduceAsync(
                KafkaGlobalConfig.TransactionNotificationTopic,
                kafkaMessage,
                ct
            );

            return deliveryResult.Status == PersistenceStatus.Persisted;
        }
        catch (ProduceException<string, string> ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Failed to deliver message: {Reason}", ex.Error.Reason);
            }

            return false;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "An error occurred while producing message to Kafka.");
            }

            return false;
        }
    }
}
