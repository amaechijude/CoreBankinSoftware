using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using KafkaMessages.NotificationMessages;

namespace AccountServices.Services;

public class TransactionToAccountConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionToAccountConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;
    public TransactionToAccountConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionToAccountConsumer> logger,
        IProducer<string, string> producer
        )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaGlobalConfig.BootstrapServers,
            GroupId = KafkaGlobalConfig.TransactionToAccountGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false // commit after db transaction succeeds
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _producer = producer;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(topic: KafkaGlobalConfig.TransactionToAccountTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var tEvent = consumeResult.Message.Value;
                    var srs = CustomMessageSerializer.Deserialize<TransactionAccountEvent>(tEvent);

                    await ProcessEvent(srs, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    if (_logger.IsEnabled(LogLevel.Critical))
                    {
                        _logger.LogCritical(
                            ex,
                            "Account service is not consuming messages: {TimeStamp}",
                            DateTimeOffset.UtcNow
                        );
                    }

                    // Send to dead letter queue
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled((LogLevel.Error)))
                        _logger.LogError(ex, "Error occured");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    private async Task ProcessEvent(TransactionAccountEvent @event, CancellationToken ct)
    {
        var handle = await HandleEvent(@event, ct);
        var notification = new NotificationEvent<TransactionAccountEvent>(@event, handle);

        var message = new Message<string, string>
        {
            Key = notification.NotificationId,
            Value = CustomMessageSerializer.Serialize(notification)
        };

        var deliveryReport = await _producer
            .ProduceAsync(
                topic: KafkaGlobalConfig.NotificationTopic,
                message: message,
                cancellationToken: ct
                );

        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // log
            // dead letter queue
        }
    }

    private async Task<bool> HandleEvent(TransactionAccountEvent transactionAccountEvent, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var operation = scope.ServiceProvider.GetRequiredService<AccountOperations>();

        return transactionAccountEvent.EventType switch
        {
            EventType.Transfer => await operation.HandleTransfer(transactionAccountEvent, ct),
            EventType.Credit => await operation.HandleCredit(transactionAccountEvent, ct),
            EventType.Debit => await operation.HandleDebit(transactionAccountEvent, ct),
            EventType.Utility => await operation.HandleUtility(transactionAccountEvent, ct),
            _ => false,
        };
    }
}
