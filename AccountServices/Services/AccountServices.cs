using AccountServices.Data;
using AccountServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedGrpcContracts.Protos.Account.V1;

namespace AccountServices.Services;

public class AccountServices(AccountDbContext dbContext) : AccountGrpcApiService.AccountGrpcApiServiceBase
{
    private readonly AccountDbContext _dbContext = dbContext;
    public override async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest request, Grpc.Core.ServerCallContext context)
    {
        var (cleanedPhone, error) = CleanedPhoneNumber(request.PhoneNumber);
        if (error is not null || cleanedPhone is null)
            return ApiResponse.Error(error!);

        var validator = new AccountRequestValidators();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return ApiResponse.Error(validationResult.Errors.Select(x => new { x.ErrorMessage, x.AttemptedValue }));

        if (!Guid.TryParse(request.CustomerId, out Guid id))
            return ApiResponse.Error("Invalid Customer");

        if (await IsExistingAccount(id))
            return ApiResponse.Error("Duplicate Account");

        var newAccount = Account.Create(id, cleanedPhone);
        await _dbContext.Accounts.AddAsync(newAccount);
        await _dbContext.SaveChangesAsync();

        return ApiResponse.Success(newAccount);
    }

    public override async Task<GetAccountResponse> GetAccountById(GetAccountByCustomerIdRequest request, Grpc.Core.ServerCallContext context)
    {
        if (!Guid.TryParse(request.AccountId, out Guid id))
            return ApiResponse.GetAccountError("Invalid Customer");

        Account? account = await _dbContext.Accounts
           .FirstOrDefaultAsync(a => a.CustomerId == id);
        if (account is null)
            return ApiResponse.GetAccountError("Account not found");

        return ApiResponse.GetSuccess(account);
    }

    public override async Task<GetAccountResponse> GetAccountByNumber(GetAccountByPhoneAccountNumberRequest request, Grpc.Core.ServerCallContext context)
    {
        var (cleanedPhone, error) = CleanedPhoneNumber(request.PhoneNumber);
        if (error is not null || cleanedPhone is null)
            return ApiResponse.GetAccountError(error!);

        Account? account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.PhoneAccountNumber == cleanedPhone);
        if (account is null)
            return ApiResponse.GetAccountError("Account not found");

        return ApiResponse.GetSuccess(account);
    }

    private async Task<bool> IsExistingAccount(Guid customerId) =>
        await _dbContext.Accounts.AnyAsync(a => a.CustomerId == customerId);

    private static (string? phone, string? error) CleanedPhoneNumber(string phone)
    {
        phone = phone.Trim();
        if (!phone.All(char.IsDigit))
            return (null, "Phone number is not all digit");
        if (phone.Length != 11 || !phone.StartsWith('0'))
            return (null, "phone number does not start with 0");

        // remove leading 0
        return (phone[1..], null);
    }
}
