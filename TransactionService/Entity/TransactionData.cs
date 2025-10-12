using System.Data;
using TransactionService.DTOs;
using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionData
{
    public Guid Id { get; private set; }
    public string TransactionRefrence { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "NGN";
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


    // static method to create
    public static TransactionData Create(
        string idempotencyKey,
        Guid customerId,
        string sessionId,
        string refrence,
        FundCreditTransferRequest request,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        TransactionCategory transactionCategory,
        TransactionStatus transactionStatus
        )
    {
        return new TransactionData
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            TransactionRefrence = refrence,
            IdempotencyKey = idempotencyKey,
            CustomerId = customerId,

            Amount = request.Amount,
            Narration = request.Narration,
            SourceAccountNumber = request.SenderAccountNumber,
            SourceBankName = request.SenderBankName,
            SourceAccountName = request.SenderAccountName,
            SourceBankNubanCode = request.SenderBankNubanCode,
            DestinationAcountNumber = request.DestinationAccountNumber,
            DestinationBankName = request.DestinationBankName,
            DestinationAccountName = request.DestinationAccountName,
            DestinationBankNubanCode = request.DestinationBankNubanCode,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            Longitude = request.Longitude,
            Latitude = request.Latitude,


            TransactionType = transactionType,
            TransactionChannel = transactionChannel,
            TransactionCategory = transactionCategory,
            TransactionStatus = transactionStatus,
        };
    }

    public void UpdateStatus(TransactionStatus newStatus, string? reason = null)
    {
        var oldStatus = TransactionStatus;
        TransactionStatus = newStatus;

        if (newStatus == TransactionStatus.Completed || newStatus == TransactionStatus.Failed)
        {
            ProcessedAt = DateTimeOffset.UtcNow;
        }

        StatusLogs.Add(new TransactionStatusLog
        {
            Id = Guid.CreateVersion7(),
            TransactionId = Id,
            TransactionReference = TransactionRefrence,
            PreviousStatus = oldStatus,
            NewStatus = newStatus,
            ChangeReason = reason ?? string.Empty,
        });
    }
}
