using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data.FluentApiConfig;

public sealed class OutBoxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(om => om.Id);

        builder.Property(om => om.TransactionId).IsRequired();
        builder.HasIndex(om => om.TransactionId);
    }
}
