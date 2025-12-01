namespace TransactionService.CustomBackgroundService;

public class KafkaMessagePublisher(ILogger<KafkaMessagePublisher> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
