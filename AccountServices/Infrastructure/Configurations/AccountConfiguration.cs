using AccountServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountServices.Infrastructure.Configurations
{
  public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
  {
    public void Configure(EntityTypeBuilder<Account> b)
    {
      b.ToTable("accounts");
      b.HasKey(x => x.Id);
      b.Property(x => x.AccountNumber).IsRequired().HasMaxLength(20);
      b.HasIndex(x => x.AccountNumber).IsUnique();
      b.Property(x => x.CustomerId).IsRequired();
      b.HasIndex(x => x.PhoneNumber).IsUnique();
      b.Property(x => x.Balance).HasColumnType("decimal(18,2)");
      b.Property(x => x.Status).HasConversion<string>();
    }
  }
}
