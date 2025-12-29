using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Entity;
using TransactionService.Services;

namespace TransactionService.Workers;

public sealed class NotificationWithOutboxWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<NotificationWithOutboxWorker> logger,
    IProducer<string, string> kafkaProducer
) : BackgroundService
{
    private static readonly string _topic = KafkaGlobalConfig.TransactionNotificationTopic;
    private const int MaxBatchSize = 100;
    private const int MaxParallelism = 10;
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ActiveQueueDelay = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessOutboxMessages(stoppingToken);

                // OPTIMIZATION 1: Adaptive polling - quick retry if queue has items
                var delay = processedCount > 0 ? ActiveQueueDelay : EmptyQueueDelay;
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in NotificationWithOutbox worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task<int> ProcessOutboxMessages(CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        var userPreference = scope.ServiceProvider.GetRequiredService<UserPreferenceService>();

        var pendingMessages = await dbContext
            .OutboxMessages.AsNoTracking()
            .Where(o => o.Status == Entity.OutboxStatus.Pending)
            .OrderBy(o => o.CreatedAt)
            .Take(MaxBatchSize)
            .ToListAsync(ct);

        if (pendingMessages.Count == 0)
        {
            return 0;
        }

        // OPTIMIZATION 2: Pre-fetch all required preferences in batch
        var allPreferences = await PreFetchAllPreferences(pendingMessages, userPreference, ct);

        // OPTIMIZATION 3: Group messages for parallel processing
        var messageGroups = pendingMessages
            .Select(m => new MessageProcessingContext
            {
                Message = m,
                SenderPreference = GetSenderPreference(m, allPreferences),
                BeneficiaryPreference = GetBeneficiaryPreference(m, allPreferences),
            })
            .ToList();

        // OPTIMIZATION 4: Process messages in parallel with controlled concurrency
        var publishTasks = new List<Task<MessagePublishResult>>(pendingMessages.Count);
        var successfulTransactionIds = new List<Guid>(pendingMessages.Count);

        await Parallel.ForEachAsync(
            messageGroups,
            new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism, CancellationToken = ct },
            async (context, token) =>
            {
                var result = await ProcessMessageAsync(context, token);
                if (result.Success)
                {
                    lock (successfulTransactionIds)
                    {
                        successfulTransactionIds.Add(result.TransactionId);
                    }
                }
            }
        );

        // OPTIMIZATION 5: Batch update all successful messages
        if (successfulTransactionIds.Count > 0)
        {
            await userPreference.MarkOutboxPublishedBatch(successfulTransactionIds, ct);
        }

        // var failedCount = pendingMessages.Count - successfulTransactionIds.Count;
        return successfulTransactionIds.Count;
    }

    // OPTIMIZATION 6: Single batch fetch for all preferences needed
    private static async Task<PreferenceCache> PreFetchAllPreferences(
        List<OutboxMessage> messages,
        UserPreferenceService userPreference,
        CancellationToken ct
    )
    {
        // Collect all unique customer IDs
        var customerIds = messages.Select(m => m.CustomerId).Distinct().ToList();

        // Collect all unique destination account numbers (for transfers)
        var destinationAccounts = messages
            .Where(m => !string.IsNullOrWhiteSpace(m.DestinationAccountNumber))
            .Select(m => m.DestinationAccountNumber!)
            .Distinct()
            .ToList();

        // Fetch in parallel
        var customerTask = userPreference.BatchGetByCustomerIds(customerIds, ct);
        var accountTask =
            destinationAccounts.Count > 0
                ? userPreference.BatchGetByAccountNumbers(destinationAccounts, ct)
                : Task.FromResult(new Dictionary<string, UserNotificationPreference>());

        await Task.WhenAll(customerTask, accountTask);

        return new PreferenceCache
        {
            ByCustomerId = await customerTask,
            ByAccountNumber = await accountTask,
        };
    }

    private static UserNotificationPreference? GetSenderPreference(
        OutboxMessage message,
        PreferenceCache cache
    )
    {
        cache.ByCustomerId.TryGetValue(message.CustomerId, out var preference);
        return preference;
    }

    private static UserNotificationPreference? GetBeneficiaryPreference(
        OutboxMessage message,
        PreferenceCache cache
    )
    {
        if (string.IsNullOrEmpty(message.DestinationAccountNumber))
        {
            return null;
        }

        cache.ByAccountNumber.TryGetValue(message.DestinationAccountNumber, out var preference);
        return preference;
    }

    private async Task<MessagePublishResult> ProcessMessageAsync(
        MessageProcessingContext context,
        CancellationToken ct
    )
    {
        var message = context.Message;

        // Validate we have required preferences
        if (context.SenderPreference is null)
        {
            return MessagePublishResult.Failed(message.TransactionId);
        }

        if (message.TransactionType == Entity.Enums.TransactionType.Transfer)
        {
            return await ProcessTransferAsync(context, ct);
        }
        else
        {
            return await ProcessOtherTransactionAsync(context, ct);
        }
    }

    private async Task<MessagePublishResult> ProcessOtherTransactionAsync(
        MessageProcessingContext context,
        CancellationToken ct
    )
    {
        var message = context.Message;
        var eventType = message.TransactionType switch
        {
            Entity.Enums.TransactionType.Deposit => EventType.Deposit,
            Entity.Enums.TransactionType.Withdrawal => EventType.Withdrawal,
            Entity.Enums.TransactionType.Debit => EventType.Debit,
            Entity.Enums.TransactionType.Credit => EventType.Credit,
            _ => EventType.Utility,
        };

        var accountEvent = CreateEvent(message, context.SenderPreference!, eventType);
        var success = await ProduceMessageAsync(accountEvent, ct);

        return success
            ? MessagePublishResult.Succeeded(message.TransactionId)
            : MessagePublishResult.Failed(message.TransactionId);
    }

    private async Task<MessagePublishResult> ProcessTransferAsync(
        MessageProcessingContext context,
        CancellationToken ct
    )
    {
        var message = context.Message;

        if (context.BeneficiaryPreference is null)
        {
            return MessagePublishResult.Failed(message.TransactionId);
        }

        var senderEvent = CreateEvent(message, context.SenderPreference!, EventType.TransferDebit);
        var beneficiaryEvent = CreateEvent(
            message,
            context.BeneficiaryPreference,
            EventType.TransferCredit
        );

        // Publish both events in parallel
        var senderTask = ProduceMessageAsync(senderEvent, ct);
        var beneficiaryTask = ProduceMessageAsync(beneficiaryEvent, ct);

        await Task.WhenAll(senderTask, beneficiaryTask);

        var senderSuccess = await senderTask;
        var beneficiarySuccess = await beneficiaryTask;

        // Both must succeed for transfer to be considered successful
        return (senderSuccess && beneficiarySuccess)
            ? MessagePublishResult.Succeeded(message.TransactionId)
            : MessagePublishResult.Failed(message.TransactionId);
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
                logger.LogError(
                    ex,
                    "Failed to deliver Kafka message for TransactionId: {TransactionId}, Reason: {Reason}",
                    accountEvent.TransactionId,
                    ex.Error.Reason
                );
            }

            return false;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Unexpected error producing Kafka message for TransactionId: {TransactionId}",
                    accountEvent.TransactionId
                );
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

    // Helper classes for better organization
    private sealed record PreferenceCache
    {
        public Dictionary<Guid, UserNotificationPreference> ByCustomerId { get; init; } = [];
        public Dictionary<string, UserNotificationPreference> ByAccountNumber { get; init; } = [];
    }

    private sealed record MessageProcessingContext
    {
        public required OutboxMessage Message { get; init; }
        public UserNotificationPreference? SenderPreference { get; init; }
        public UserNotificationPreference? BeneficiaryPreference { get; init; }
    }

    private sealed class MessagePublishResult
    {
        public Guid TransactionId { get; init; }
        public bool Success { get; init; }

        public static MessagePublishResult Succeeded(Guid transactionId) =>
            new() { TransactionId = transactionId, Success = true };

        public static MessagePublishResult Failed(Guid transactionId) =>
            new() { TransactionId = transactionId, Success = false };
    }
}
