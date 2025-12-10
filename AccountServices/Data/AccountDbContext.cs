using AccountServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Data;

public sealed class AccountDbContext(DbContextOptions<AccountDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
    }
}
