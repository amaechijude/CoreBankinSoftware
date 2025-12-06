using AccountServices.Data;
using KafkaMessages.AccountMessages;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Services;

internal class AccountOperations(AccountDbContext dbContext, ILogger<AccountOperations> logger)
{
    internal async Task<bool> HandleTransfer(TransactionAccountEvent @event, CancellationToken ct)
    {
        var retries = 7;
        var strategy = dbContext.Database.CreateExecutionStrategy();

        while (retries > 0)
        {
            try
            {
                var execute = await strategy.ExecuteAsync<bool>(async () =>
                {
                    await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
                    try
                    {
                        var customerAccount = await dbContext.Accounts
                            .FirstOrDefaultAsync(a => a.CustomerId == @event.CustomerId, ct);
                        var beneficiaryAccount = await dbContext.Accounts
                            .FirstOrDefaultAsync(a =>
                                a.AccountNumber == @event.DestinationAccountNumber, ct);
                        if (customerAccount is null || beneficiaryAccount is null)
                            return false;

                        customerAccount.DebitAccount(@event.Amount);
                        await dbContext.SaveChangesAsync(ct);

                        beneficiaryAccount.CreditAccount(@event.Amount);
                        await dbContext.SaveChangesAsync(ct);

                        await dbTransaction.CommitAsync(ct); // unit of work
                        return true;
                    }
                    catch (Exception e)
                    {
                        await dbTransaction.RollbackAsync(ct);
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.LogError(e, "exceptions occured");
                        throw;
                    }

                });

                return execute;
            }
            catch (DBConcurrencyException)
            {
                retries--;
                if (retries == 0) return false;

                await Task.Delay(70, ct);
            }

        }


        return false;
    }
    internal async Task<bool> HandleCredit(TransactionAccountEvent @event, CancellationToken ct)
    {
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                var account = await dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.CustomerId == @event.CustomerId, ct);

                account?.CreditAccount(@event.Amount);
                await dbContext.SaveChangesAsync(ct);
                return true;
            }
            catch (DBConcurrencyException)
            {
                retries--;
                if (retries == 0) return false;
                await Task.Delay(100, ct);
            }
        }
        return false;
    }

    internal async Task ProcessEvent(TransactionAccountEvent @event, CancellationToken ct)
    {

    }
    internal async Task<bool> HandleDebit(TransactionAccountEvent @event, CancellationToken ct)
    {
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                var account = await dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.CustomerId == @event.CustomerId, ct);
                if (account is null) return false;

                account.DebitAccount(@event.Amount);
                await dbContext.SaveChangesAsync(ct);
                return true;
            }
            catch (DBConcurrencyException)
            {
                retries--;
                if (retries == 0) return false;
                await Task.Delay(100, ct);
            }
        }

        return false;
    }

    internal async Task<bool> HandleUtility(TransactionAccountEvent @event, CancellationToken ct)
    {
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                var account = await dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.CustomerId == @event.CustomerId, ct);
                if (account is null) return false;

                account.DebitAccount(@event.Amount);
                await dbContext.SaveChangesAsync(ct);
                return true;
            }
            catch (DBConcurrencyException)
            {
                retries--;
                if (retries == 0) return false;
                await Task.Delay(100, ct);
            }
        }

        return false;
    }
}
