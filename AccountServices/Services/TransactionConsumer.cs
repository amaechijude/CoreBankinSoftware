using Confluent.Kafka;
using KafkaMessages;
using Microsoft.Extensions.Logging;

namespace AccountServices.Services;

public class TransactionToAccountConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionToAccountConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    public TransactionToAccountConsumer(IServiceScopeFactory scopeFactory, ILogger<TransactionToAccountConsumer> logger)
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
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(topic: KafkaGlobalConfig.TransactionToAccountTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                }
                catch (ConsumeException ex)
                {
                    if (_logger.IsEnabled(LogLevel.Critical))
                        _logger.LogCritical(ex, "Account service is not consuming messages: {TimeStamp}",
                            DateTimeOffset.UtcNow);

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
}
