using CustomerProfile.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class VerificationCodeCleanupService(
    IServiceProvider serviceProvider,
    ILogger<VerificationCodeCleanupService> logger
) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<VerificationCodeCleanupService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run the cleanup job every hour
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();

                var deletedCount = await context
                    .VerificationCodes.Where(vc => vc.ExpiresAt < DateTimeOffset.UtcNow)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deletedCount > 0)
                {
                    _logger.LogInformation(
                        "Cleaned up {Count} expired verification codes.",
                        deletedCount
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cleaning up verification codes.");
            }
        }
    }
}
