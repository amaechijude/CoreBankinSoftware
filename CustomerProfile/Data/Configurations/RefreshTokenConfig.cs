using CustomerProfile.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerProfile.Data.Configurations;

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.HasIndex(rt => rt.Token);
        builder.HasIndex(rt => rt.UserId);

        // string
        builder.Property(rt => rt.Token).HasMaxLength(1_000).IsRequired();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(100);
    }
}
