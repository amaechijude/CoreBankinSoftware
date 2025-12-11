using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerAPI.Data.Configurations
{
    public class VerificationCodesConfig : IEntityTypeConfiguration<VerificationCode>
    {
        public void Configure(EntityTypeBuilder<VerificationCode> builder)
        {
            builder.ToTable("VerificationCodes");
            builder.Property(vc => vc.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<CustomGuidV7Generator>();

            builder.HasKey(vc => vc.Id);
            builder.HasIndex(vc => vc.UserPhoneNumber);
        }
    }
}
