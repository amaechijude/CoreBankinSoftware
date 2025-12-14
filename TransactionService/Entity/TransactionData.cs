using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public sealed class TransactionData
{
    public Guid Id { get; private init; }
    public string TransactionReference { get; private init; } = string.Empty;
    public string IdempotencyKey { get; private init; } = string.Empty;
    public Guid CustomerId { get; private init; }
    public string? DestinationAccountNumber { get; private init; }
    public string? DestinationBankName { get; private init; }
    public uint RowVersion { get; set; }

    public decimal Amount { get; private init; }
    public string? Narration { get; private init; } = string.Empty;

    public TransactionType TransactionType { get; private init; }
    public string? TransactionChannel { get; private init; }
    public TransactionCategory TransactionCategory { get; private init; }
    public TransactionStatus TransactionStatus { get; private set; } = TransactionStatus.Initiated;
    public CurrencyType Currency { get; private init; } = CurrencyType.NGN;
    public decimal TransactionFee { get; private init; } = 0;
    public decimal ValueAddedTax { get; private init; } = 0;

    // Timestamps
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    // Audit fields
    public string SessionId { get; private init; } = string.Empty;
    public string DeviceInfo { get; private init; } = string.Empty;
    public string IpAddress { get; private init; } = string.Empty;
    public string? Longitude { get; private init; }
    public string? Latitude { get; private init; }

    public ICollection<TransactionStatusLog> TransactionStatusLogs { get; set; } = [];

    // static method to create
    public static TransactionData Create(
        FundCreditTransferRequest request,
        TransactionType transactionType,
        string reference,
        TransactionCategory category,
        string sessionId
    )
    {
        var txn = new TransactionData
        {
            Id = Guid.CreateVersion7(),
            TransactionReference = reference,
            CustomerId = request.CustomerId,
            IdempotencyKey = request.IdempotencyKey,
            DestinationAccountNumber = request.DestinationAccountNumber,
            DestinationBankName = request.DestinationBankName,
            Amount = request.Amount,
            Narration = request.Narration,
            TransactionType = transactionType,
            TransactionStatus = TransactionStatus.Initiated,
            TransactionCategory = category,
            TransactionChannel = request.TransactionChannel,

            SessionId = sessionId,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            TransactionStatusLogs = [],
        };
        var log = TransactionStatusLog.Create(txn, TransactionStatus.Initiated, "Initiated");
        txn.TransactionStatusLogs.Add(log);
        return txn;
    }

    public void UpdateStatus(TransactionStatus status, string description)
    {
        var log = TransactionStatusLog.Create(this, status, description);
        TransactionStatusLogs.Add(log);
        TransactionStatus = status;
    }
}
