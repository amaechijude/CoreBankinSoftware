using Confluent.Kafka;
using KafkaMessages;

namespace Notification.Services;

public sealed class NotificationBackgroundProcessor : BackgroundService
{
    private readonly ILogger<NotificationBackgroundProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public NotificationBackgroundProcessor(ILogger<NotificationBackgroundProcessor> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaGlobalConfig.BootstrapServers,
            GroupId = KafkaGlobalConfig.NotificationTopic,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

    }

}
