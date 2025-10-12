using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionDisputeConfig : IEntityTypeConfiguration<TransactionDispute>
{
    public void Configure(EntityTypeBuilder<TransactionDispute> builder)
    {
        builder.ToTable("TransactionDisputes");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.DisputeReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.DisputeReference).IsUnique();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference);

        builder.Property(t => t.DisputeType).HasConversion<string>().IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
