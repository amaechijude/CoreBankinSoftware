using System.Security.AccessControl;
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
        var validator = new AccountRequestValidators();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return ServiceResponse.Error(validationResult.Errors.Select(x => new { x.ErrorMessage, x.AttemptedValue }));

        if (!Guid.TryParse(request.CustomerId, out Guid id))
            return ServiceResponse.Error("Invalid Customer");

        if (await IsExistingAccount(id))
            return ServiceResponse.Error("Duplicate Account");

        var newAccount = Account.Create(id, request.PhoneNumber);
        await _dbContext.Accounts.AddAsync(newAccount);
        await _dbContext.SaveChangesAsync();

        return ServiceResponse.Success(newAccount);
    }

    public override async Task<GetAccountResponse> GetAccountById(GetAccountByCustomerIdRequest request, Grpc.Core.ServerCallContext context)
    {
        if (!Guid.TryParse(request.AccountId, out Guid id))
            return ServiceResponse.GetAccountError("Invalid Customer");

        Account? account = await _dbContext.Accounts
           .FirstOrDefaultAsync(a => a.CustomerId == id);
        if (account is null)
            return ServiceResponse.GetAccountError("Account not found");

        return ServiceResponse.GetSuccess(account);
    }

    public override async Task<GetAccountResponse> GetAccountByNumber(GetAccountByPhoneAccountNumberRequest request, Grpc.Core.ServerCallContext context)
    {
      Account? account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.PhoneAccountNumber ==  CleanedPhoneNumber(request.PhoneNumber));
        if (account is null)
            return ServiceResponse.GetAccountError("Account not found");

        return ServiceResponse.GetSuccess(account);
    }

    private async Task<bool> IsExistingAccount(Guid customerId) =>
        await _dbContext.Accounts.AnyAsync(a => a.CustomerId == customerId);

    private static string CleanedPhoneNumber(string phone)
    {
        phone = phone.Trim();
        return phone[1..];
    }
}

public static class ServiceResponse
{
    public static CreateAccountResponse Success(Account account)
    {
        return new CreateAccountResponse
        {
            Success = true,
            AccountId = account.Id.ToString(),
            AccountNumber = account.PhoneAccountNumber,
            AccountBalance = (double)account.Balance,
        };
    }
    public static CreateAccountResponse Error(object error)
    {
        return new CreateAccountResponse
        {
            Success = false,
            Error = error.ToString()
        };
    }

    public static GetAccountResponse GetSuccess(Account account)
    {
        return new GetAccountResponse
        {
            Success = true,
            AccountId = account.Id.ToString(),
            AccountNumber = account.PhoneAccountNumber,
            AccountBalance = (double)account.Balance,
            IsActive = account.Status == Domain.Enums.AccountStatus.Active,
            CanTransact = account.IsOnPostNoDebit,
            Error = string.Empty
        };
    }

    public static GetAccountResponse GetAccountError(string error)
    {
        return new GetAccountResponse
        {
            Success = false,
            Error = error
        };
    }
}
