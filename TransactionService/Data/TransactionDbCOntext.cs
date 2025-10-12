using Microsoft.EntityFrameworkCore;
using TransactionService.Entity;

namespace TransactionService.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{

    public DbSet<TransactionData> Transactions { get; set; }
    public DbSet<RecurringTransactionSchedule> RecurringTransactionSchedules { get; set; }
    public DbSet<TransactionDispute> TransactionDisputes { get; set; }
    public DbSet<TransactionFeeBreakdown> TransactionFeeBreakdowns { get; set; }
    public DbSet<TransactionHold> TransactionHolds { get; set; }
    public DbSet<TransactionNotification> TransactionNotifications { get; set; }
    public DbSet<TransactionReversal> TransactionReversals { get; set; }
    public DbSet<TransactionStatusLog> TransactionStatusLogs { get; set; }
    public DbSet<TransactionNibssDetail> TransactionNibssDetails { get; set; }

    // Configure entity Fluent API configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}