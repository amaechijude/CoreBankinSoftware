using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Entity;

namespace TransactionService.Data;

public class RecurringTransactionScheduleConfig : IEntityTypeConfiguration<RecurringTransactionSchedule>
{
    public void Configure(EntityTypeBuilder<RecurringTransactionSchedule> builder)
    {
        builder.ToTable("RecurringTransactionSchedules");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CustomGuidV7Generator>();

        builder.Property(t => t.ScheduleReference).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.ScheduleReference).IsUnique();

        builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(t => t.Frequency).HasConversion<string>().IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
