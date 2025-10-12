using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionConfig : IEntityTypeConfiguration<TransactionData>
{
    public void Configure(EntityTypeBuilder<TransactionData> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        // auto-generate GUIDs
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.TransactionRefrence).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionRefrence).IsUnique();

        builder.Property(t => t.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.IdempotencyKey).IsUnique();

        builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(t => t.TransactionFee).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(t => t.ValueAddedTax).IsRequired().HasColumnType("decimal(18,2)");

        // Enums as strings
        builder.Property(t => t.TransactionType).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionChannel).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionStatus).HasConversion<string>().IsRequired();
        builder.Property(t => t.Currency).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionCategory).HasConversion<string>().IsRequired();
    }
}