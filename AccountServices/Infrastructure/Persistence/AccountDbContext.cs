using AccountServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Infrastructure.Persistence
{
  public sealed class AccountDbContext : DbContext
  {
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
    }
  }
}
