using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class Transaction
{
    public Guid Id { get; private set; }
    public string RefrenceNumber { get; private set; } = string.Empty;

    public Decimal Amount { get; private set; }
    public string? Narration { get; private set; } = string.Empty;

    public string SourceAccountNumber { get; private set; } = string.Empty;
    public string SourceBankName { get; private set; } = string.Empty;
    public string SourceAccountName { get; private set; } = string.Empty;
    public string BeneficiaryAcountNumber { get; private set; } = string.Empty;
    public string BeneficiaryBankName { get; private set; } = string.Empty;
    public string BeneficiaryAccountName { get; private set; } = string.Empty;

    public TransactionType TransactionType { get; private set; }
    public TransactionChannel TransactionChannel { get; private set; }
    public TransactionStatus TransactionStatus { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public Decimal? SourceAccountBalanceBefore { get; private set; }
    public Decimal? SourceAccountBalanceAfter { get; private set; }

    public string SessionId { get; private set; } = string.Empty;
    public string DeviceInfo { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string InitiatedBy { get; private set; } = string.Empty;

    public string? FailureReason { get; private set; }
    public Decimal TransactionFee { get; private set; }

}
