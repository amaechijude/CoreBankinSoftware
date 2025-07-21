using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Shared.Domain.Entities;

namespace CustomerProfile.Infrastructure.Data.Configurations
{
    public partial class CustomerConfigurations : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Ensure the correct namespace for 'ToTable' is included
            builder.ToTable("Customers");

            // Configure primary key
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

            // Index customer number
            builder.HasIndex(c => c.CustomerNumber);

            // Configure properties
            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(18);

            builder.Property(c => c.FirstName)
                .IsRequired();
            builder.Property(c => c.LastName)
                .IsRequired();

            // Configure owned collections and Enums in separate partial class
            ConfigureOwnedCollections(builder);
            COnfigureEnums(builder);
        }
    }
}
