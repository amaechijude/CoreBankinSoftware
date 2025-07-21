using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Shared.Domain.Entities;
using src.Shared.Infrastructure.Data;

namespace CustomerProfile.Infrastructure.Data.Configurations
{
    public partial class CustomerConfigurations : IEntityTypeConfiguration<Customer>
    {
        private static void ConfigureOwnedCollections(EntityTypeBuilder<Customer> builder)
        {
            // Configure Owned Collections
            // Addresses
            builder.OwnsMany(c => c.Addresses, a =>
            {
                a.ToTable("Addresses");
                a.WithOwner().HasForeignKey("CustomerId");
                a.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                a.HasKey("Id");
                a.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(100);
                a.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(50);
                a.Property(a => a.State)
                    .IsRequired()
                    .HasMaxLength(50);
                a.Property(a => a.AddressType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                a.Property(a => a.VerificationMethod)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            builder.OwnsMany(c => c.NextOfKins, n =>
            {
                n.ToTable("NextOfKins");
                n.WithOwner().HasForeignKey("CustomerId");
                n.Property<Guid>("Id")
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
                k.WithOwner().HasForeignKey("CustomerId");
                k.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                k.HasKey("Id");
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
                r.WithOwner().HasForeignKey("CustomerId");
                r.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                r.HasKey("Id");
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
                cc.WithOwner().HasForeignKey("CustomerId");
                cc.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                cc.HasKey("Id");
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
