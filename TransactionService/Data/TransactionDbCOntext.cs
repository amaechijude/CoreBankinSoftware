using Microsoft.EntityFrameworkCore;
using TransactionService.Entity;

namespace TransactionService.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{

    public DbSet<TransactionData> Transactions => Set<TransactionData>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}