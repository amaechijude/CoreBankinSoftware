using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class RecurringTransactionSchedule
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ScheduleReference { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string SourceAccountNumber { get; private set; } = string.Empty;
    public string DestinationAcountNumber { get; private set; } = string.Empty;
    public string DestinationBankName { get; private set; } = string.Empty;
    public string DestinationAccountName { get; private set; } = string.Empty;
    public string DestinationBankNubanCode { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public ScheduleFrequency Frequency { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public DateTimeOffset? NextRunDate { get; private set; }
    public ScheduleStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
