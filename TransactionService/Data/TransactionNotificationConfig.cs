using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data;

public class TransactionNotificationConfig : IEntityTypeConfiguration<TransactionNotification>
{
    public void Configure(EntityTypeBuilder<TransactionNotification> builder)
    {
        builder.ToTable("TransactionNotifications");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionReference);

        builder.Property(t => t.RecipientType).HasConversion<string>().IsRequired();
        builder.Property(t => t.NotificationType).HasConversion<string>().IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
