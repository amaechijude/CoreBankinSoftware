using CustomerAPI.Data;
using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerAPI.Infrastructure.Data.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        // key and index
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.PhoneNumber).IsUnique();

        // Configure relationships explicitly
        builder
            .HasMany(c => c.Addresses)
            .WithOne(a => a.UserProfile)
            .HasForeignKey(a => a.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // enums
        builder.Property(c => c.CustomerType).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.KYCStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.AccountTier).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.EmploymentType).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Source).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Gender).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.MaritalStatus).HasConversion<string>().HasMaxLength(20);

        // strings profile
        builder.Property(c => c.UserAccountNumber).HasMaxLength(10).IsFixedLength();
        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
        builder.Property(c => c.MiddleName).HasMaxLength(100);
        builder.Property(c => c.MaidenName).HasMaxLength(100);
        builder.Property(c => c.PlaceOfBirth).HasMaxLength(100);
        builder.Property(c => c.StateOfOrigin).HasMaxLength(100);
        builder.Property(c => c.Nationality).HasMaxLength(100);
        builder.Property(c => c.Title).HasMaxLength(10);

        // string auth & identifier
        builder.Property(c => c.PasswordHash).HasMaxLength(1024).IsRequired();
        builder.Property(c => c.Username).HasMaxLength(250).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(250).IsRequired();
        builder.Property(c => c.AlternateEmail).HasMaxLength(250);
        builder.Property(c => c.PhoneNumber).HasMaxLength(17).IsRequired();
        builder.Property(c => c.AlternatePhoneNumber).HasMaxLength(17);

        // string and PII hash
        builder.Property(c => c.BvnHash).HasMaxLength(1024);
        builder.Property(c => c.NinHash).HasMaxLength(1024);

        // Configure Owned Collections

        // Nin Data
        builder.OwnsOne(
            c => c.NinData,
            nin =>
            {
                nin.ToTable("NinData");
                nin.WithOwner(n => n.Customer).HasForeignKey(c => c.CustomerId);

                nin.Property<Guid>(nk => nk.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                nin.HasKey(k => k.Id);
                nin.HasIndex(k => k.Id).IsUnique();
            }
        );

        // Bvn Data
        builder.OwnsOne(
            c => c.BvnData,
            bvn =>
            {
                bvn.ToTable("BvnData");
                bvn.WithOwner(n => n.UserProfile).HasForeignKey(c => c.UserProfileId);

                bvn.Property<Guid>(nk => nk.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                bvn.HasKey(k => k.Id);
                bvn.HasIndex(k => k.Id).IsUnique();
            }
        );

        // Next of Kin
        builder.OwnsMany(
            c => c.NextOfKins,
            n =>
            {
                n.ToTable("NextOfKins");
                n.WithOwner(k => k.Customer).HasForeignKey(k => k.CustomerId);

                n.Property<Guid>(nk => nk.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                n.HasKey(k => k.Id);

                n.Property(n => n.Gender).HasConversion<string>().HasMaxLength(20);
                n.Property(n => n.Relationship).HasConversion<string>().HasMaxLength(50);
                n.Property(n => n.Category).HasConversion<string>().HasMaxLength(20);
            }
        );

        // kyc documents
        builder.OwnsMany(
            c => c.KycDocuments,
            k =>
            {
                k.ToTable("KYCDocuments");
                k.WithOwner(ky => ky.Customer).HasForeignKey(ky => ky.CustomerId);

                k.Property<Guid>(ky => ky.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                k.HasKey(ky => ky.Id);
                k.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(50);
                k.Property(d => d.DocumentMimeType).HasConversion<string>().HasMaxLength(50);
            }
        );

        // risk asssessment
        builder.OwnsMany(
            c => c.RiskAssessments,
            r =>
            {
                r.ToTable("RiskAssessments");
                r.WithOwner(r => r.Customer).HasForeignKey(r => r.CustomerId);

                r.Property<Guid>(r => r.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();
                r.HasKey(r => r.Id);
                r.Property(r => r.RiskAssessmentType).HasConversion<string>().HasMaxLength(50);
                r.Property(r => r.RiskAssessmentSummary).HasConversion<string>().HasMaxLength(50);
                r.Property(r => r.RiskLevel).HasConversion<string>().HasMaxLength(50);
            }
        );

        // Compliance checks
        builder.OwnsMany(
            c => c.ComplianceChecks,
            cc =>
            {
                cc.ToTable("ComplianceChecks");
                cc.WithOwner(c => c.UserProfile).HasForeignKey(c => c.UserProfileId);

                cc.Property<Guid>(c => c.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<CustomGuidV7Generator>();

                cc.HasKey(c => c.Id);
                cc.Property(c => c.CheckType).HasConversion<string>().HasMaxLength(50);
                cc.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);
            }
        );
    }
}
