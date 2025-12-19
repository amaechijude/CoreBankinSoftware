using System.Text.Json;
using Hangfire.PostgreSql.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Quartz.Xml.JobSchedulingData20;
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

        var grpcProfile = await FetchFromGrpcApi(customerId, ct);
        return grpcProfile!;
    }

    public async Task<UserNotificationPreference> GetByCustomerAccountNumber(
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
        var grpcProfile = await FetchFromGrpcApi(accountNumber, ct);
        return grpcProfile!;
    }

    private async Task<UserNotificationPreference?> FetchFromGrpcApi(
        Guid customerId,
        CancellationToken ct
    )
    {
        var request = new GetCustomerPrefrenceRequestById { CustomerId = customerId.ToString() };
        var options = new Grpc.Core.CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(15),
            cancellationToken: ct
        );
        var response = await grpcClient.GetCustmonerPrefrenceByIdAsync(request, options);

        if (response is null || !response.Success)
        {
            return null;
        }
        var newresponse = new PreferenceRequestResponseBody(
            CustomerId: Guid.Parse(response.CustomerId),
            Email: response.Email,
            PhoneNumber: response.PhoneNumber,
            AccountNumber: response.BeneficiaryAccountNumber,
            FirstName: response.FirstName,
            LastName: response.LastName
        );
        var prf = UserNotificationPreference.Create(newresponse);
        await AddToDbAndCache(prf, ct);
        return prf;
    }

    private async Task<UserNotificationPreference?> FetchFromGrpcApi(
        string accountNumber,
        CancellationToken ct
    )
    {
        var request = new GetCustomerPrefrenceRequestByAccountNumber
        {
            AccountNumber = accountNumber,
        };
        var options = new Grpc.Core.CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(15),
            cancellationToken: ct
        );
        var response = await grpcClient.GetNotificatiosPrefrencesByAccountNumberAsync(
            request,
            options
        );

        if (response is null || !response.Success)
        {
            return null;
        }
        var newresponse = new PreferenceRequestResponseBody(
            CustomerId: Guid.Parse(response.CustomerId),
            Email: response.Email,
            PhoneNumber: response.PhoneNumber,
            AccountNumber: response.BeneficiaryAccountNumber,
            FirstName: response.FirstName,
            LastName: response.LastName
        );
        var prf = UserNotificationPreference.Create(newresponse);
        await AddToDbAndCache(prf, ct);
        return prf;
    }

    private async Task AddToDbAndCache(UserNotificationPreference request, CancellationToken ct)
    {
        // add to db
        try
        {
            dbContext.UserNotificationPreferences.Add(request);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // ignore duplicate key errors
        }
        finally
        {
            // add to cache
            var cacheKey = $"customer_preference_{request.CustomerId}";
            var valueToCache = JsonSerializer.SerializeToUtf8Bytes(request);
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
    }
}
