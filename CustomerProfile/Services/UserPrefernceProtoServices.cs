using CustomerProfile.Data;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;

namespace CustomerProfile.Services;

public sealed class UserPrefernceProtoServices(UserProfileDbContext context)
    : CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceBase
{
    private readonly UserProfileDbContext _context = context;

    public override async Task<GetCustomerPrefrenceResponse> GetCustmonerPrefrenceById(
        GetCustomerPrefrenceRequestById request,
        ServerCallContext context
    )
    {
        if (!Guid.TryParse(request.CustomerId, out var customerId))
        {
            return new GetCustomerPrefrenceResponse { };
        }
        var preference = await _context.UserProfiles.FindAsync(
            [customerId],
            cancellationToken: context.CancellationToken
        );
        if (preference is null)
        {
            return new GetCustomerPrefrenceResponse { };
        }
        return new GetCustomerPrefrenceResponse
        {
            Success = true,
            AccountNumber = preference.UserAccountNumber,
            FirstName = preference.FirstName,
            LastName = preference.LastName,
            Email = preference.Email,
            PhoneNumber = preference.PhoneNumber,
            IsEmailEnabled = true,
            IsSmsEnabled = true,
            BankName = "Core Bank",
            CustomerId = preference.Id.ToString(),
        };
    }

    public override async Task<BatchGetCustomerPrefrenceResponse> BatchGetCustmonerPrefrenceByIds(
        BatchGetCustomerPrefrenceRequestByIds request,
        ServerCallContext context
    )
    {
        List<Guid> customerIds =
        [
            .. request
                .CustomerId.Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                .Where(guid => guid != Guid.Empty),
        ];
        var batchResult = await _context
            .UserProfiles.Where(up => customerIds.Contains(up.Id))
            .AsNoTracking()
            .Select(preference => new
            {
                preference.UserAccountNumber,
                preference.FirstName,
                preference.LastName,
                preference.Email,
                preference.PhoneNumber,
                preference.Id,
            })
            .ToListAsync(context.CancellationToken);

        var preferences = batchResult.Select(r => new GetCustomerPrefrenceResponse
        {
            Success = true,
            IsEmailEnabled = true,
            IsSmsEnabled = true,
            BankName = "Core Bank",
            AccountNumber = r.UserAccountNumber,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            CustomerId = r.Id.ToString(),
        });
        return new BatchGetCustomerPrefrenceResponse { CustomerPreferences = { preferences } };
    }

    public override async Task<GetCustomerPrefrenceResponse> GetNotificatiosPrefrencesByAccountNumber(
        GetCustomerPrefrenceRequestByAccountNumber request,
        ServerCallContext context
    )
    {
        var preference = await _context
            .UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.UserAccountNumber == request.AccountNumber,
                cancellationToken: context.CancellationToken
            );
        if (preference is null)
        {
            return new GetCustomerPrefrenceResponse { };
        }
        return new GetCustomerPrefrenceResponse
        {
            Success = true,
            AccountNumber = preference.UserAccountNumber,
            FirstName = preference.FirstName,
            LastName = preference.LastName,
            Email = preference.Email,
            PhoneNumber = preference.PhoneNumber,
            IsEmailEnabled = true,
            IsSmsEnabled = true,
            BankName = "Core Bank",
            CustomerId = preference.Id.ToString(),
        };
    }

    public override async Task<BatchGetCustomerPrefrenceResponse> BatchGetNotificatiosPrefrencesByAccountNumber(
        BatchGetCustomerPrefrenceRequestByAccountNumbers request,
        ServerCallContext context
    )
    {
        var batchResult = await _context
            .UserProfiles.Where(up => request.AccountNumber.Contains(up.UserAccountNumber))
            .AsNoTracking()
            .Select(preference => new
            {
                preference.UserAccountNumber,
                preference.FirstName,
                preference.LastName,
                preference.Email,
                preference.PhoneNumber,
                preference.Id,
            })
            .ToListAsync(context.CancellationToken);

        var preferences = batchResult.Select(r => new GetCustomerPrefrenceResponse
        {
            Success = true,
            IsEmailEnabled = true,
            IsSmsEnabled = true,
            BankName = "Core Bank",
            AccountNumber = r.UserAccountNumber,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            CustomerId = r.Id.ToString(),
        });
        return new BatchGetCustomerPrefrenceResponse { CustomerPreferences = { preferences } };
    }
}
