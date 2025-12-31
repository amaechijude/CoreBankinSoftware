using CustomerProfile.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerProfile.Data.Configurations;

public sealed class AddressConfig : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);

        // strings
        builder.Property(a => a.BuildingNumber).HasMaxLength(20);
        builder.Property(a => a.Street).HasMaxLength(100);
        builder.Property(a => a.Landmark).HasMaxLength(500);
        builder.Property(a => a.City).HasMaxLength(100);
        builder.Property(a => a.LocalGovernmentArea).HasMaxLength(200);
        builder.Property(a => a.State).HasMaxLength(200);
        builder.Property(a => a.Country).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(10);
        //
        builder.Property(a => a.VerifiedBy).HasMaxLength(500);
        builder.Property(a => a.VerificationReference).HasMaxLength(500);
    }
}
