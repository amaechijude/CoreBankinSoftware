using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public class TransactionStatusLogConfig : IEntityTypeConfiguration<TransactionStatusLog>
{
    public void Configure(EntityTypeBuilder<TransactionStatusLog> builder)
    {
        builder.ToTable("TransactionStatusLogs");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference);

        builder.Property(t => t.PreviousStatus).HasConversion<string>();
        builder.Property(t => t.NewStatus).HasConversion<string>().IsRequired();
    }
}
