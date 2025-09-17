using AccountServices.Domain.Entities;

namespace AccountServices.Application.Interfaces
{
  public interface IAccountRepository
  {
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
        Task<Account?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
        Task AddAsync(Account account, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
  }
}
