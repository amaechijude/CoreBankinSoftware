using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data;

public class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        // auto-generate GUIDs
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.RefrenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");

        // Enums as strings
        builder.Property(t => t.TransactionType).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionChannel).HasConversion<string>().IsRequired();
        builder.Property(t => t.TransactionStatus).HasConversion<string>().IsRequired();

    }
}