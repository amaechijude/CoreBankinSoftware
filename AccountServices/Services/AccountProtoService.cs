using AccountServices.Data;
using AccountServices.Domain.Entities;
using AccountServices.Domain.Enums;
using AccountServices.Validators;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SharedGrpcContracts.Protos.Account.Operations.V1;

namespace AccountServices.Services;

public sealed class AccountProtoService(
    AccountDbContext dbContext,
    CreateAccountRequestValidator validator,
    CustomResiliencePolicy resiliencePolicy
    ) : AccountOperationsGrpcService.AccountOperationsGrpcServiceBase
{
    public override async Task<AccountOperationResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
    {
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            var error = string.Join(", ", validate.Errors.Select(x => x.ErrorMessage));
            return ApiResponseFactory.Error(error);
        }
        var customerId = Guid.Parse(request.CustomerId);
        var accountType = SwitchAccountType(request.AccountTypeRequest);

        var newAccount = Account.Create(customerId, request.PhoneNumber, accountType, request.AccountName);
        dbContext.Accounts.Add(newAccount);
        try
        {
            await dbContext.SaveChangesAsync();
            return ApiResponseFactory.Success();
        }
        catch (DbUpdateException)
        {
            var error = $"Duplicate {accountType} account";
            return ApiResponseFactory.Error(error);
        }
        catch (Exception)
        {
            var error = "Account not created";
            // log and/or retry;
            return ApiResponseFactory.Error(error);
        }
    }

    public override async Task<IntraBankNameEnquiryResponse> IntraBankNameEnquiry(IntraBankNameEnquiryRequest request, ServerCallContext context)
    {
        var accountNumber = request.AccountNumber;
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length != 10)
            return ApiResponseFactory.Failed("Invalid Account number");

        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);

        return account is null
            ? ApiResponseFactory.Failed("Account not found")
            : ApiResponseFactory.Success(account);
    }

    public override async Task<AccountOperationResponse> Deposit(DepositRequest request, ServerCallContext context)
    {
        var amount = (decimal)request.Amount;
        if (amount < 50)
            return ApiResponseFactory.Error("Minimum deposit is 50");

        // update in one database query
        var rowsAffected = await dbContext.Accounts
            .Where(a => a.AccountNumber == request.AccountNumber && a.Status == AccountStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Balance, a => a.Balance + amount)
                .SetProperty(a => a.UpdatedAt, DateTimeOffset.UtcNow)
            );

        if (rowsAffected == 0)
        {
            // Check if account exists or just Inactive
            var accountExists = await dbContext.Accounts
                .AnyAsync(a => a.AccountNumber == request.AccountNumber);

            return ApiResponseFactory.Error(accountExists ? "Account Inactive" : "Account not found");
        }

        return ApiResponseFactory.Success();
    }

    public override async Task<AccountOperationResponse> Withdraw(WithdrawRequest request, ServerCallContext context)
    {
        // handle concurrency with polly
        return await resiliencePolicy.DbConcurrencyRetryPolicy
            .ExecuteAsync<AccountOperationResponse>(async () =>
            {

                var amount = (decimal)request.Amount;

                // fetch from db without tracking
                var account = await dbContext.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);

                if (account is null)
                    return ApiResponseFactory.Error("Account not found");
                if (account.IsInsufficient(amount))
                    return ApiResponseFactory.Error("Insufficient funds");
                if (account.IsOnPostNoDebit)
                    return ApiResponseFactory.Error("Witdhrawal forbidden, Visit your bank");

                // update in one round trip
                var rowsAffected = await dbContext.Accounts
                    .Where(a =>
                        a.AccountNumber == account.AccountNumber
                        && a.RowVersion == account.RowVersion
                    )
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Balance, a => a.Balance - amount)
                        .SetProperty(a => a.UpdatedAt, DateTimeOffset.UtcNow)
                );

                if (rowsAffected == 0)
                    throw new DbUpdateConcurrencyException(""); // Polly will handle retries

                return ApiResponseFactory.Success();
            }
        );
    }

    public override Task<AccountOperationResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        return base.Transfer(request, context);
    }

    public override Task<AccountOperationResponse> Reserve(ReserveRequest request, ServerCallContext context)
    {
        return base.Reserve(request, context);
    }

    public override Task<AccountOperationResponse> Release(ReleaseRequest request, ServerCallContext context)
    {
        return base.Release(request, context);
    }

    private static AccountType SwitchAccountType(AccountTypeRequest accountType)
    {
        return accountType switch
        {
            AccountTypeRequest.Business => AccountType.Business,
            _ => AccountType.Personal
        };
    }
}

public static class ApiResponseFactory
{
    public static AccountOperationResponse Success()
    => new() { Success = true };

    public static AccountOperationResponse Error(string error)
    => new() { Success = false, Error = error };

    public static IntraBankNameEnquiryResponse Failed(string error)
    => new() { Success = false, Error = error };

    public static IntraBankNameEnquiryResponse Success(Account account)
    => new()
    {
        Success = true,
        AccountName = account.AccountName,
        AccountNumber = account.AccountNumber,
        BankName = account.BankName
    };
}