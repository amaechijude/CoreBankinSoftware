using AccountServices.Domain.Entities;

namespace AccountServices.Application.Interfaces
{
  public interface IAccountRepository
  {
    Task<Account?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
  }
}
