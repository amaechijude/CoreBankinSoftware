using AccountServices.Application.Interfaces;
using AccountServices.Domain.Entities;
using AccountServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Infrastructure.Repositories
{
  public sealed class AccountRepository : IAccountRepository
  {
    private readonly AccountDbContext _db;
    public AccountRepository(AccountDbContext db) => _db = db;

    public Task<Account?> GetAsync(Guid id, CancellationToken ct = default)
      => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
      => _db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
      => await _db.Accounts.AddAsync(account, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
      => _db.SaveChangesAsync(ct);
  }
}
