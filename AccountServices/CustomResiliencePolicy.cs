using AccountServices.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using SharedGrpcContracts.Protos.Account.Operations.V1;

namespace AccountServices;

public sealed class CustomResiliencePolicy
{
    private static AsyncRetryPolicy DbConcurrencyRetryPolicy =>
        Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    context["retryCount"] = retryCount;
                }
            );

    // Wrap retry with fallback
    public IAsyncPolicy<AccountOperationResponse> DbConcurrencyRetryWithFallback =>
        Policy<AccountOperationResponse>
            .Handle<DbUpdateConcurrencyException>()
            .FallbackAsync(
                fallbackValue: ApiResponseFactory.Error(
                    "Unable to complete operation due to high transaction volume. Please try again."
                ),
                onFallbackAsync: async (outcome, context) =>
                {
                    await Task.CompletedTask;
                }
            )
            .WrapAsync(DbConcurrencyRetryPolicy);
}
