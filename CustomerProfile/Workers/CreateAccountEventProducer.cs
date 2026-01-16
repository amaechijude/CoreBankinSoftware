using Confluent.Kafka;

namespace CustomerProfile.Workers;

public sealed class CreateAccountEventProducer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<CreateAccountEventProducer> logger,
    IProducer<string, string> kafkaProducer
) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateAccountEventProducer> _logger = logger;
    private readonly IProducer<string, string> _kafkaProducer = kafkaProducer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    // private async Task ProduceMesageAsync()
}

public record CreateAccountChannelMessage
{
    public required string AccountName { get; init; }
    public required string PhoneNumber { get; init; }
}
