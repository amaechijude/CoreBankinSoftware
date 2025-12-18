using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;
using TransactionService.Data;
using TransactionService.Entity;

namespace TransactionService.Services;

public sealed class UserPreferenceService(
    TransactionDbContext dbContext,
    IDistributedCache distributedCache,
    CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient grpcClient
)
{
    public async Task<Dictionary<string, UserNotificationPreference>?> GetDetailsForTransfer(
        Guid customerId,
        string accountNumber,
        CancellationToken ct
    )
    {
        var task1 = GetByCustomerId(customerId, ct);
        var task2 = GetByCustomerAccountNumber(accountNumber, ct);

        await Task.WhenAll(task1, task2);
        var profile1 = task1.Result;
        var profile2 = task2.Result;

        if (profile1 is null || profile2 is null)
        {
            return null;
        }
        Dictionary<string, UserNotificationPreference> response = new(capacity: 2);
        response.TryAdd(profile1.CustomerId.ToString(), profile1);
        response.TryAdd(profile2.AccountNumber, profile2);

        return response;
    }

    public async Task<UserNotificationPreference> GetByCustomerId(
        Guid customerId,
        CancellationToken ct
    )
    {
        // get from cache
        var cacheKey = $"customer_preference_{customerId}";
        var cachedBytes = await distributedCache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            var profile = JsonSerializer.Deserialize<UserNotificationPreference>(cachedBytes);
            return profile!;
        }

        // fallback to db
        var profile1 = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CustomerId == customerId, ct);

        if (profile1 is not null)
        {
            var valueToCache = JsonSerializer.SerializeToUtf8Bytes(profile1);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20),
            };

            await distributedCache.SetAsync(
                key: cacheKey,
                value: valueToCache,
                options: cacheOptions,
                token: ct
            );
        }

        // fallback to api call
        return profile1!;
    }

    private async Task<UserNotificationPreference> GetByCustomerAccountNumber(
        string accountNumber,
        CancellationToken ct
    )
    {
        // get from cache
        var cacheKey = $"customer_account_{accountNumber}";
        var cachedBytes = await distributedCache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            var profile = JsonSerializer.Deserialize<UserNotificationPreference>(cachedBytes);
            return profile!;
        }

        // fallback to db
        var profile1 = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountNumber == accountNumber, ct);

        if (profile1 is not null)
        {
            var valueToCache = JsonSerializer.SerializeToUtf8Bytes(profile1);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20),
            };

            await distributedCache.SetAsync(
                key: cacheKey,
                value: valueToCache,
                options: cacheOptions,
                token: ct
            );
        }

        // fallback to api call
        return profile1!;
    }

    private async Task<Dictionary<string, UserNotificationPreference>?> FetchFromGrpcApi(
        Guid customerId,
        string accountNumber,
        CancellationToken ct
    )
    {
        var request = new GetNotificatiosForTransferRequest
        {
            CustomerId = customerId.ToString(),
            BeneficiaryAccountNumber = accountNumber,
        };
        var options = new Grpc.Core.CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(15),
            cancellationToken: ct
        );
        var response = await grpcClient.GetNotificatiosForTransferAsync(request, options);

        if (response is null || response.CustomerPrefrences.Count == 0 || !response.Success)
        {
            return null;
        }

        Dictionary<string, UserNotificationPreference> result = new(
            response.CustomerPrefrences.Count
        );
        // foreach (var item in response.CustomerPrefrences)
        // {
        //     result.TryAdd(item.CustomerId, item);
        //     result.TryAdd(item, item);
        // }

        return result;
    }
}
