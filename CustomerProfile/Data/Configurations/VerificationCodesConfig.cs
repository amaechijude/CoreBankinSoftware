using CustomerProfile.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerProfile.Data.Configurations;

public sealed class VerificationCodesConfig : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        builder.ToTable("VerificationCodes");
        builder
            .Property(vc => vc.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.HasKey(vc => vc.Id);
        builder.HasIndex(vc => vc.UserPhoneNumber);

        // strings
        builder.Property(vc => vc.Code).IsRequired().HasMaxLength(7);
        builder.Property(vc => vc.UserEmail).IsRequired().HasMaxLength(200);
        builder.Property(vc => vc.UserPhoneNumber).IsRequired().HasMaxLength(17);
    }
}
