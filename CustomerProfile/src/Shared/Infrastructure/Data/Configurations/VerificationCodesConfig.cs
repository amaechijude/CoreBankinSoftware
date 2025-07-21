using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Shared.Domain.Entities;

namespace src.Shared.Infrastructure.Data.Configurations
{
    public class VerificationCodesConfig : IEntityTypeConfiguration<VerificationCode>
    {
        public void Configure(EntityTypeBuilder<VerificationCode> builder)
        {
            builder.ToTable("VerificationCodes");
            builder.HasKey(vc => vc.Code);
            builder.HasIndex(vc => vc.UserPhoneNumber);
        }
    }
}
