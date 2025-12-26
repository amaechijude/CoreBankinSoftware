using Confluent.Kafka;
using KafkaMessages;
using KafkaMessages.AccountMessages;
using NotificationWorkerService.Email;

namespace NotificationWorkerService;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaGlobalConfig.BootstrapServers,
            GroupId = KafkaGlobalConfig.NotificationGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(KafkaGlobalConfig.NotificationTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Broker.WaitForKafkaAsync(KafkaGlobalConfig.BootstrapServers, _logger, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var notificationEvent =
                    CustomMessageSerializer.Deserialize<TransactionAccountEvent>(
                        consumeResult.Message.Value
                    );

                await ProcessEmail(notificationEvent, stoppingToken);
                _consumer.Commit();
            }
            catch (OperationCanceledException)
            {
                _consumer.Close();
                _consumer.Dispose();
                break;
            }
            catch (ConsumeException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Notification consumer is not consuming events");
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception) { }
        }
    }

    private async Task<bool> ProcessEmail(TransactionAccountEvent @event, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<EmailService>();
        var emailRequest = EmailTemplateGenerator.CreateEmailRequests(@event);
        return await service.SendEmailAsync(emailRequest, ct);
    }
}
