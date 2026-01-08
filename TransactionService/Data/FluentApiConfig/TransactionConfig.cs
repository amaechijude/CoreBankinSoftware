using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public sealed class TransactionConfig : IEntityTypeConfiguration<TransactionData>
{
    public void Configure(EntityTypeBuilder<TransactionData> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);

        // strings
        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(150);
        builder.HasIndex(t => t.TransactionReference).IsUnique();

        builder.Property(t => t.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.IdempotencyKey).IsUnique();

        // monies
        builder.Property(t => t.Amount).IsRequired().HasPrecision(19, 4);
        builder.Property(t => t.TransactionFee).IsRequired().HasPrecision(19, 4);
        builder.Property(t => t.ValueAddedTax).IsRequired().HasPrecision(19, 4);

        // Enums as strings
        builder.Property(t => t.TransactionType).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionChannel).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionStatus).HasConversion<string>().IsRequired();
        builder.Property(t => t.Currency).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionCategory).HasConversion<string>().IsRequired();

        // owned entities
        builder.OwnsMany(
            t => t.TransactionStatusLogs,
            logs =>
            {
                logs.ToTable("transaction_status_logs");
                logs.WithOwner(st => st.TransactionData).HasForeignKey(st => st.TransactionId);
                logs.HasKey(st => st.Id);
                logs.Property(st => st.CurrentStatus).HasConversion<string>();
                logs.Property(st => st.PreviousStatus).HasConversion<string>();
            }
        );
    }
}
