using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionFeeBreakdown
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TransactionReference { get; private set; } = string.Empty;
    public FeeType FeeType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
