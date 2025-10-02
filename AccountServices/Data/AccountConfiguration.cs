using AccountServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountServices.Data
{
    public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("accounts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CustomerId).IsRequired();
            builder.HasIndex(x => x.CustomerId).IsUnique();

            builder.Property(x => x.PhoneAccountNumber).IsRequired();
            builder.HasIndex(x => x.PhoneAccountNumber).IsUnique();

            builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Status).HasConversion<string>();
        }
    }
}
