using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Domain.Entities;
using src.Infrastructure.Data;

namespace CustomerProfile.Infrastructure.Data.Configurations
{
    public partial class CustomerConfigurations : IEntityTypeConfiguration<Customer>
    {
        private static void ConfigureOwnedCollections(EntityTypeBuilder<Customer> builder)
        {
            // Configure Owned Collections
            
            builder.OwnsMany(c => c.NextOfKins, n =>
            {
                n.ToTable("NextOfKins");
                n.WithOwner(k => k.Customer)
                .HasForeignKey(k => k.CustomerId);

                n.Property<Guid>(nk => nk.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                n.HasKey("Id");
               
                n.Property(n => n.Gender)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                n.Property(n => n.Relationship)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                n.Property(n => n.Category)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            // kyc documents
            builder.OwnsMany(c => c.KYCDocuments, k =>
            {
                k.ToTable("KYCDocuments");
                k.WithOwner(ky => ky.Customer)
                .HasForeignKey(ky => ky.CustomerId);

                k.Property<Guid>(ky => ky.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                k.HasKey(ky => ky.Id);
                k.Property(d => d.DocumentType)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                k.Property(d => d.DocumentMimeType)
                    .HasConversion<string>()
                    .HasMaxLength(50);   
            });

            // risk asssessment
            builder.OwnsMany(c => c.RiskAssessments, r =>
            {
                r.ToTable("RiskAssessments");
                r.WithOwner(r => r.Customer)
                .HasForeignKey(r => r.CustomerId);

                r.Property<Guid>(r => r.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                r.HasKey(r => r.Id);
                r.Property(r => r.RiskAssessmentType)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                r.Property(r => r.RiskAssessmentSummary)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                r.Property(r => r.RiskLevel)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // Compliance checks
            builder.OwnsMany(c => c.ComplianceChecks, cc =>
            {
                cc.ToTable("ComplianceChecks");
                cc.WithOwner(c => c.Customer)
                    .HasForeignKey(c => c.CustomerId);
                cc.Property<Guid>(c => c.CustomerId)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();

                cc.HasKey(c => c.Id);
                cc.Property(c => c.CheckType)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                cc.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });
        }
    }
}
