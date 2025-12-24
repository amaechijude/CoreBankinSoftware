using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;
using TransactionService.Data;
using TransactionService.Entity;

namespace TransactionService.Services;

public sealed class UserPreferenceService(
    TransactionDbContext dbContext,
    HybridCache hybridCache,
    CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient grpcClient
)
{
    private static string CustomerIdKey(Guid customerId) => $"customer_preference_{customerId}";

    private static string AccountKey(string accountNumber) => $"customer_account_{accountNumber}";

    // OPTIMIZATION 1: Batch update for marking multiple messages as published
    public async Task MarkOutboxPublishedBatch(List<Guid> transactionIds, CancellationToken ct)
    {
        if (transactionIds.Count == 0)
            return;

        await dbContext
            .OutboxMessages.Where(o => transactionIds.Contains(o.TransactionId))
            .ExecuteUpdateAsync(
                s =>
                {
                    s.SetProperty(o => o.Status, OutboxStatus.Published);
                    s.SetProperty(o => o.PublishedAt, DateTimeOffset.UtcNow);
                },
                ct
            );
    }

    // Keep original for backward compatibility
    public async Task MarkOutboxPublished(Guid transactionId, CancellationToken ct)
    {
        await MarkOutboxPublishedBatch([transactionId], ct);
    }

    public async Task<UserNotificationPreference?> GetByAccountNumber(
        string accountNumber,
        CancellationToken ct
    )
    {
        var result = await hybridCache.GetOrCreateAsync(
            key: AccountKey(accountNumber),
            factory: async token => await GetByAccountNumberFactory(accountNumber, token),
            cancellationToken: ct
        );
        return result;
    }

    public async Task<UserNotificationPreference?> GetByCustomerId(
        Guid customerId,
        CancellationToken ct
    )
    {
        var result = await hybridCache.GetOrCreateAsync(
            key: CustomerIdKey(customerId),
            factory: async token => await GetByCustomerIdFactory(customerId, token),
            cancellationToken: ct
        );
        return result;
    }

    public async Task<Dictionary<string, UserNotificationPreference>> GetDetailsForTransfer(
        Guid customerId,
        string accountNumber,
        CancellationToken ct
    )
    {
        var senderTask = GetByCustomerId(customerId, ct);
        var beneficiaryTask = GetByAccountNumber(accountNumber, ct);
        await Task.WhenAll(senderTask, beneficiaryTask);

        var sender = await senderTask;
        var beneficiary = await beneficiaryTask;
        var result = new Dictionary<string, UserNotificationPreference>();
        if (sender is not null && beneficiary is not null)
        {
            result[customerId.ToString()] = sender;
            result[accountNumber] = beneficiary;
        }
        return result;
    }

    private async Task<UserNotificationPreference?> GetByAccountNumberFactory(
        string accountNumber,
        CancellationToken ct
    )
    {
        var preferences = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountNumber == accountNumber, ct);
        if (preferences is not null)
        {
            return preferences;
        }
        var request = new GetCustomerPrefrenceRequestByAccountNumber
        {
            AccountNumber = accountNumber,
        };
        var options = new Grpc.Core.CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(15),
            cancellationToken: ct
        );
        try
        {
            var response = await grpcClient.GetNotificatiosPrefrencesByAccountNumberAsync(
                request,
                options
            );
            if (response is not null)
            {
                return UserNotificationPreference.Create(
                    Guid.Parse(response.CustomerId),
                    response.Email,
                    response.PhoneNumber,
                    response.AccountNumber,
                    response.FirstName,
                    response.LastName
                );
            }
        }
        catch (Exception)
        {
            // Handle not found case if necessary
            return null;
        }
        return null;
    }

    private async Task<UserNotificationPreference?> GetByCustomerIdFactory(
        Guid customerId,
        CancellationToken ct
    )
    {
        var preferences = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CustomerId == customerId, ct);
        if (preferences is not null)
        {
            return preferences;
        }
        var request = new GetCustomerPrefrenceRequestById { CustomerId = customerId.ToString() };
        var options = new Grpc.Core.CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(15),
            cancellationToken: ct
        );
        try
        {
            var response = await grpcClient.GetCustmonerPrefrenceByIdAsync(request, options);
            if (response is not null)
            {
                return UserNotificationPreference.Create(
                    Guid.Parse(response.CustomerId),
                    response.Email,
                    response.PhoneNumber,
                    response.AccountNumber,
                    response.FirstName,
                    response.LastName
                );
            }
        }
        catch (Exception)
        {
            // Handle not found case if necessary
            return null;
        }
        return null;
    }

    // OPTIMIZATION 3: Batch fetch for multiple account numbers
    public async Task<Dictionary<string, UserNotificationPreference>> BatchGetByAccountNumbers(
        List<string> accountNumbers,
        CancellationToken ct
    )
    {
        var count = accountNumbers.Count;
        if (count == 0)
            return [];

        var preferences = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .Where(p => accountNumbers.Contains(p.AccountNumber))
            .ToListAsync(ct);

        if (preferences.Count == count) // All found in database
        {
            return preferences.ToDictionary(p => p.AccountNumber, p => p);
        }

        var foundAcc = preferences.Select(p => p.AccountNumber).ToHashSet();
        var missingAcc = accountNumbers.Where(ac => !foundAcc.Contains(ac)).ToList();

        if (missingAcc.Count > 0)
        {
            var request = new BatchGetCustomerPrefrenceRequestByAccountNumbers();
            request.AccountNumber.AddRange(missingAcc);

            var options = new Grpc.Core.CallOptions(
                deadline: DateTime.UtcNow.AddSeconds(15),
                cancellationToken: ct
            );
            var response = await grpcClient.BatchGetNotificatiosPrefrencesByAccountNumberAsync(
                request,
                options
            );
            if (response is not null && response.CustomerPreferences.Count > 0)
            {
                List<UserNotificationPreference> preferencesFromGrpc =
                [
                    .. response.CustomerPreferences.Select(x =>
                        UserNotificationPreference.Create(
                            Guid.Parse(x.CustomerId),
                            x.Email,
                            x.PhoneNumber,
                            x.AccountNumber,
                            x.FirstName,
                            x.LastName
                        )
                    ),
                ];

                preferences.AddRange(preferencesFromGrpc);
            }
        }
        return preferences.ToDictionary(p => p.AccountNumber, p => p);
    }

    // OPTIMIZATION 4: Batch fetch for multiple customer IDs
    public async Task<Dictionary<Guid, UserNotificationPreference>> BatchGetByCustomerIds(
        List<Guid> customerIds,
        CancellationToken ct
    )
    {
        var idCount = customerIds.Count;

        if (idCount == 0)
            return [];

        var preferences = await dbContext
            .UserNotificationPreferences.AsNoTracking()
            .Where(p => customerIds.Contains(p.CustomerId))
            .ToListAsync(ct);

        if (preferences.Count == idCount) // All found in database
        {
            return preferences.ToDictionary(p => p.CustomerId, p => p);
        }

        var foundIds = preferences.Select(p => p.CustomerId).ToHashSet();
        var missingIds = customerIds.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            var request = new BatchGetCustomerPrefrenceRequestByIds();
            request.CustomerId.AddRange(missingIds.Select(id => id.ToString()));

            var options = new Grpc.Core.CallOptions(
                deadline: DateTime.UtcNow.AddSeconds(15),
                cancellationToken: ct
            );
            var response = await grpcClient.BatchGetCustmonerPrefrenceByIdsAsync(request, options);
            if (response is not null && response.CustomerPreferences.Count > 0)
            {
                List<UserNotificationPreference> preferencesFromGrpc =
                [
                    .. response.CustomerPreferences.Select(x =>
                        UserNotificationPreference.Create(
                            Guid.Parse(x.CustomerId),
                            x.Email,
                            x.PhoneNumber,
                            x.AccountNumber,
                            x.FirstName,
                            x.LastName
                        )
                    ),
                ];

                preferences.AddRange(preferencesFromGrpc);
            }
        }
        return preferences.ToDictionary(p => p.CustomerId, p => p);
    }
}
