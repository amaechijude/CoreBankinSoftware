using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class Transaction
{
    public Guid Id { get; private set; }
    public string TransactionRefrence { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }

    public decimal Amount { get; private set; }
    public CurrencyType Currency { get; private set; } = CurrencyType.NGN;
    public string? Narration { get; private set; } = string.Empty;

    public string SourceAccountNumber { get; private set; } = string.Empty;
    public string SourceBankName { get; private set; } = string.Empty;
    public string SourceAccountName { get; private set; } = string.Empty;
    public string SourceBankNubanCode { get; private set; } = string.Empty;

    public string DestinationAcountNumber { get; private set; } = string.Empty;
    public string DestinationBankName { get; private set; } = string.Empty;
    public string DestinationAccountName { get; private set; } = string.Empty;
    public string DestinationBankNubanCode { get; private set; } = string.Empty;

    public TransactionType TransactionType { get; private set; }
    public TransactionChannel TransactionChannel { get; private set; }
    public TransactionCategory TransactionCategory { get; private set; }
    public TransactionStatus TransactionStatus { get; private set; } = TransactionStatus.Initiated;
    public decimal TransactionFee { get; private set; } = 0;
    public decimal ValueAddedTax { get; private set; } = 0;

    // Timestamps
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; private set; }

    public decimal? SourceAccountBalanceBefore { get; private set; }
    public decimal? SourceAccountBalanceAfter { get; private set; }

    // Audit fields
    public string SessionId { get; private set; } = string.Empty;
    public string DeviceInfo { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string InitiatedBy { get; private set; } = string.Empty;
    public string? Longitude { get; private set; }
    public string? Latitude { get; private set; }

    // Navigation properties for related entities
    public TransactionNibssDetail? NibssDetail { get; set; }
    public ICollection<TransactionStatusLog> StatusLogs { get; set; } = [];
    public ICollection<TransactionFeeBreakdown> FeeBreakdowns { get; set; } = [];
    public ICollection<TransactionNotification> Notifications { get; set; } = [];
    public ICollection<TransactionDispute> Disputes { get; set; } = [];
    public ICollection<TransactionHold> Holds { get; set; } = [];
    public ICollection<TransactionReversal> Reversals { get; set; } = [];
}
