using Microsoft.EntityFrameworkCore;
using TransactionService.Entity;

namespace TransactionService.Data;

public sealed class TransactionDbContext(DbContextOptions<TransactionDbContext> options)
    : DbContext(options)
{
    public DbSet<TransactionData> Transactions => Set<TransactionData>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences =>
        Set<UserNotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}
