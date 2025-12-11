using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using KafkaMessages.NotificationMessages;
using Notification.Workers;

namespace Notification.Services;

public sealed class NotificationBackgroundProcessor : BackgroundService
{
    private readonly ILogger<NotificationBackgroundProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public NotificationBackgroundProcessor(
        ILogger<NotificationBackgroundProcessor> logger,
        IServiceScopeFactory scopeFactory
    )
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaGlobalConfig.BootstrapServers,
            GroupId = KafkaGlobalConfig.NotificationGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(KafkaGlobalConfig.NotificationTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var notificationEvent = CustomMessageSerializer.Deserialize<
                    NotificationEvent<TransactionAccountEvent>
                >(consumeResult.Message.Value);

                await ProcessEmail(notificationEvent, stoppingToken);
                _consumer.Commit();
            }
            catch (Exception) { }
        }
    }

    private async Task<bool> ProcessEmail(
        NotificationEvent<TransactionAccountEvent> @event,
        CancellationToken ct
    )
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<EmailService>();
        return await service.SendEmailAsync(new EmailRequest("", "", "", ""), ct);
    }
}
