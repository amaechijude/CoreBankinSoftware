using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public sealed class UserNotificationCOnfig : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("notification-prefrence");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.AccountNumber).HasMaxLength(10).IsFixedLength().IsRequired();
        builder.Property(p => p.Email).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PhoneNumber).HasMaxLength(17).IsRequired();
        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100);
    }
}
