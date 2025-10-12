using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionNibssDetailConfig : IEntityTypeConfiguration<TransactionNibssDetail>
{
    public void Configure(EntityTypeBuilder<TransactionNibssDetail> builder)
    {
        builder.ToTable("TransactionNibssDetails");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference).IsUnique();
    }
}
