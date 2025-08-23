using CustomerAPI.Data;
using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerProfile.Infrastructure.Data.Configurations
{
    public partial class UserConfig : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            // Ensure the correct namespace for 'ToTable' is included
            builder.ToTable("UserProfiles");


            // Configure primary key
            builder.Property(c => c.Id)
                   .ValueGeneratedOnAdd()
                   .HasValueGenerator<CustomGuidV7Generator>();

            builder.HasKey(c => c.Id);

            // Configure Value Objects
            builder.OwnsOne(c => c.BVN, b =>
            {
                b.Property(bvn => bvn.Value)
                    .HasColumnName("BVN");
            });
            builder.OwnsOne(c => c.NIN, n =>
            {
                n.Property(nin => nin.Value)
                    .HasColumnName("NIN");
            });


            // Configure relationships explicitly
            builder.HasMany(c => c.Addresses)
                   .WithOne(a => a.Customer)
                   .HasForeignKey("CustomerId")
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure owned collections and Enums in separate partial class
            ConfigureOwnedCollections(builder);
            ConfigureEnums(builder);
        }
    }
}
