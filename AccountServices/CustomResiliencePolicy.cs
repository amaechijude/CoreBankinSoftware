using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace AccountServices;

public sealed class CustomResiliencePolicy
{
    public AsyncRetryPolicy DbConcurrencyRetryPolicy
        => Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)
                )
        );
}
