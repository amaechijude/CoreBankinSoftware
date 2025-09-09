using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerAPI.Data.Configurations
{
    public class AccountConfig : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.Property<Guid>(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<CustomGuidV7Generator>();
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountTier)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(a => a.UserProfile)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.CustomerId);
        }
    }
}
