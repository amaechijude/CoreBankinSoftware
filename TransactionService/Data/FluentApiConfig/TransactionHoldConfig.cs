using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionHoldConfig : IEntityTypeConfiguration<TransactionHold>
{
    public void Configure(EntityTypeBuilder<TransactionHold> builder)
    {
        builder.ToTable("TransactionHolds");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.HoldReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.HoldReference).IsUnique();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference);

        builder.Property(t => t.HoldAmount).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(t => t.HoldType).HasConversion<string>().IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
