using AccountServices.Domain.Entities;
using SharedGrpcContracts.Protos.Account.V1;

namespace AccountServices.Services;


public static class ApiResponse
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

