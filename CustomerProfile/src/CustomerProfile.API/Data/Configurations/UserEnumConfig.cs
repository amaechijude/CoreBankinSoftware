using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerProfile.Infrastructure.Data.Configurations
{
    public partial class UserConfig : IEntityTypeConfiguration<User>
    {
        public static void ConfigureEnums(EntityTypeBuilder<User> builder)
        {
            // Configure enums to string
            builder.Property(c => c.AccountTier)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(c => c.CustomerType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(c => c.KYCStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(c => c.EmploymentType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(c => c.Source)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(c => c.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);
        }
    }
}