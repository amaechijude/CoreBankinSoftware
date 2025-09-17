using AccountServices.Application.Interfaces;
using AccountServices.Domain.Entities;
using AccountServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Infrastructure.Repositories;

public sealed class AccountRepository(AccountDbContext db) : IAccountRepository
{
  private readonly AccountDbContext _db = db;

  public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
 {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

  public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
     {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);
    }

  public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        await _db.Accounts.AddAsync(account, ct);
    }

  public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Account?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
    {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber, ct);
    }
}