using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace TransactionService.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }

    // Configure entity Fluent API configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}