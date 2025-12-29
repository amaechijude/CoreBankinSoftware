using AccountServices.Data;
using AccountServices.Entities;
using AccountServices.Entities.Enums;
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
    public override async Task<AccountOperationResponse> CreateAccount(
        CreateAccountRequest request,
        ServerCallContext context
    )
    {
        var validate = await validator.ValidateAsync(request, context.CancellationToken);
        if (!validate.IsValid)
        {
            var error = string.Join(", ", validate.Errors.Select(x => x.ErrorMessage));
            return ApiResponseFactory.Error(error);
        }
        var customerId = Guid.Parse(request.CustomerId);
        var accountType = SwitchAccountType(request.AccountTypeRequest);

        var newAccount = Account.Create(
            customerId,
            request.PhoneNumber,
            accountType,
            request.AccountName
        );
        dbContext.Accounts.Add(newAccount);
        try
        {
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return ApiResponseFactory.Success();
        }
        catch (DbUpdateException)
        {
            var error = $"Duplicate {accountType} account";
            return ApiResponseFactory.Error(error);
        }
        catch (Exception)
        {
            return ApiResponseFactory.Error("Account creation failed");
        }
    }

    public override async Task<IntraBankNameEnquiryResponse> IntraBankNameEnquiry(
        IntraBankNameEnquiryRequest request,
        ServerCallContext context
    )
    {
        var accountNumber = request.AccountNumber;
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length != 10)
        {
            return ApiResponseFactory.Failed("Invalid Account number");
        }

        var account = await dbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, context.CancellationToken);

        return account is null
            ? ApiResponseFactory.Failed("Account not found")
            : ApiResponseFactory.Success(account);
    }

    public override async Task<AccountOperationResponse> Deposit(
        DepositRequest request,
        ServerCallContext context
    )
    {
        return await resiliencePolicy.DbConcurrencyRetryWithFallback.ExecuteAsync(async () =>
        {
            try
            {
                var amount = (decimal)request.Amount;
                if (amount < 50)
                {
                    return ApiResponseFactory.Error("Minimum deposit is 50");
                }

                var account = await dbContext.Accounts.FirstOrDefaultAsync(
                    a => a.AccountNumber == request.AccountNumber,
                    context.CancellationToken
                );

                if (account is null)
                {
                    return ApiResponseFactory.Error("Account not found");
                }

                account.CreditAccount(amount);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                return ApiResponseFactory.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw; // Re-throw to allow Polly to handle the retry
            }
            catch (Exception)
            {
                return ApiResponseFactory.Error("Deposit failed");
            }
        });
    }

    public override async Task<AccountOperationResponse> Withdraw(
        WithdrawRequest request,
        ServerCallContext context
    )
    {
        // handle concurrency with polly
        return await resiliencePolicy.DbConcurrencyRetryWithFallback.ExecuteAsync(async () =>
        {
            try
            {
                var amount = (decimal)request.Amount;

                var account = await dbContext.Accounts.FirstOrDefaultAsync(
                    a => a.AccountNumber == request.AccountNumber,
                    context.CancellationToken
                );

                if (account is null)
                {
                    return ApiResponseFactory.Error("Account not found");
                }

                if (account.IsInsufficient(amount))
                {
                    return ApiResponseFactory.Error("Insufficient funds");
                }

                if (account.IsOnPostNoDebit)
                {
                    return ApiResponseFactory.Error("Witdhrawal forbidden, Visit your bank");
                }

                account.DebitAccount(amount);
                await dbContext.SaveChangesAsync(context.CancellationToken);

                return ApiResponseFactory.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw; // Re-throw to allow Polly to handle the retry
            }
            catch (Exception)
            {
                return ApiResponseFactory.Error("Withdrawal failed");
            }
        });
    }

    public override async Task<AccountOperationResponse> Transfer(
        TransferRequest request,
        ServerCallContext context
    )
    {
        return await resiliencePolicy.DbConcurrencyRetryWithFallback.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                context.CancellationToken
            );

            try
            {
                var amount = (decimal)request.Amount;
                if (amount < 50)
                {
                    return ApiResponseFactory.Error("Transfer amount must be 50 and above");
                }

                if (!Guid.TryParse(request.CustomerId, out var customerId))
                {
                    return ApiResponseFactory.Error("Invalid customer id");
                }

                // Fetch and lock the source and destination accounts
                var fromAccount = await dbContext.Accounts.FirstOrDefaultAsync(
                    a => a.CustomerId == customerId,
                    context.CancellationToken
                );

                var toAccount = await dbContext.Accounts.FirstOrDefaultAsync(
                    a => a.AccountNumber == request.ToAccountNumber,
                    context.CancellationToken
                );

                if (fromAccount is null)
                {
                    return ApiResponseFactory.Error("Source account not found.");
                }

                if (toAccount is null)
                {
                    return ApiResponseFactory.Error("Destination account not found.");
                }

                if (fromAccount.IsInsufficient(amount))
                {
                    return ApiResponseFactory.Error("Insufficient funds.");
                }

                if (fromAccount.IsOnPostNoDebit)
                {
                    return ApiResponseFactory.Error(
                        "Withdrawal forbidden on source account. Visit your bank."
                    );
                }

                if (toAccount.Status != AccountStatus.Active)
                {
                    return ApiResponseFactory.Error("Destination account is not active.");
                }

                // Perform the transfer
                fromAccount.DebitAccount(amount);
                toAccount.CreditAccount(amount);

                await dbContext.SaveChangesAsync(context.CancellationToken);
                await transaction.CommitAsync(context.CancellationToken);

                return ApiResponseFactory.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(context.CancellationToken);
                throw; // Re-throw to allow Polly to handle the retry
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(context.CancellationToken);
                return ApiResponseFactory.Error("Transfer failed");
            }
        });
    }

    private static AccountType SwitchAccountType(AccountTypeRequest accountType)
    {
        return accountType switch
        {
            AccountTypeRequest.Business => AccountType.Business,
            _ => AccountType.Personal,
        };
    }
}

public static class ApiResponseFactory
{
    public static AccountOperationResponse Success() => new() { Success = true };

    public static AccountOperationResponse Error(string error) =>
        new() { Success = false, Error = error };

    public static IntraBankNameEnquiryResponse Failed(string error) =>
        new() { Success = false, Error = error };

    public static IntraBankNameEnquiryResponse Success(Account account) =>
        new()
        {
            Success = true,
            AccountName = account.AccountName,
            AccountNumber = account.AccountNumber,
            BankName = account.BankName,
        };
}
