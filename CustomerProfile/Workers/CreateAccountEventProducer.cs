using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CustomerProfile.Workers;

public sealed class CreateAccountEventProducer(IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
