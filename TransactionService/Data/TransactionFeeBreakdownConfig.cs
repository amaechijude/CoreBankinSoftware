using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data;

public class TransactionFeeBreakdownConfig : IEntityTypeConfiguration<TransactionFeeBreakdown>
{
    public void Configure(EntityTypeBuilder<TransactionFeeBreakdown> builder)
    {
        builder.ToTable("TransactionFeeBreakdowns");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference);

        builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(t => t.FeeType).HasConversion<string>().IsRequired();
    }
}
