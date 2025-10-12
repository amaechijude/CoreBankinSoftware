using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionReversalConfig : IEntityTypeConfiguration<TransactionReversal>
{
    public void Configure(EntityTypeBuilder<TransactionReversal> builder)
    {
        builder.ToTable("TransactionReversals");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.ReversalReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.ReversalReference).IsUnique();

        builder.Property(t => t.OriginalTransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.OriginalTransactionReference);

        builder.Property(t => t.ReversalTransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.ReversalTransactionReference);

        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
