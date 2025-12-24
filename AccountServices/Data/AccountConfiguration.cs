using AccountServices.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountServices.Data;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.HasIndex(x => new { x.CustomerId, x.AccountType });

        // strings
        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(10).IsFixedLength();
        builder.HasIndex(x => x.AccountNumber).IsUnique();

        builder.Property(x => x.PhoneNumber).HasMaxLength(17);

        builder.Property(x => x.AccountName).HasMaxLength(200);

        builder.Property(x => x.BankName).HasMaxLength(200);
        // Amounts
        builder.Property(x => x.Balance).HasPrecision(19, 4);
        builder.Property(x => x.ReservedAmount).HasPrecision(19, 4);

        // enums
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(20);

        // Row version
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
