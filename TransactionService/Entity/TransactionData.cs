using System.ComponentModel.DataAnnotations;
using System.Data;
using Npgsql.Replication;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity.Enums;

namespace TransactionService.Entity;

public class TransactionData
{
    public Guid Id { get; private init; }
    public string TransactionReference { get; private init; } = string.Empty;
    public string IdempotencyKey { get; private init; } = string.Empty;
    public Guid CustomerId { get; private init; }
    public uint RowVersion { get; set; }

    public decimal Amount { get; private init; }
    public string? Narration { get; private init; } = string.Empty;

    public string AccountNumber { get; private init; } = string.Empty;
    public string BankName { get; private init; } = string.Empty;
    public string AccountName { get; private init; } = string.Empty;
    public string BankNubanCode { get; private init; } = string.Empty;

    public TransactionType TransactionType { get; private init; }
    public TransactionChannel TransactionChannel { get; private init; }
    public TransactionCategory TransactionCategory { get; private init; }
    public TransactionStatus TransactionStatus { get; private set; } = TransactionStatus.Initiated;
    public CurrencyType Currency { get; private init; } = CurrencyType.NGN;
    public decimal TransactionFee { get; private init; } = 0;
    public decimal ValueAddedTax { get; private init; } = 0;

    // Timestamps
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }

    // Audit fields
    public string SessionId { get; private init; } = string.Empty;
    public string DeviceInfo { get; private init; } = string.Empty;
    public string IpAddress { get; private init; } = string.Empty;
    public string? Longitude { get; private init; }
    public string? Latitude { get; private init; }

    public ICollection<TransactionStatusLog> TransactionStatusLogs { get; set; } = [];


    // static method to create
    public static TransactionData Create(
        string idempotencyKey,
        Guid customerId,
        string sessionId,
        string reference,
        FundCreditTransferRequest request,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        TransactionCategory transactionCategory,
        TransactionStatus transactionStatus
        )
    {
        var txn = new TransactionData
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            TransactionReference = reference,
            IdempotencyKey = idempotencyKey,
            CustomerId = customerId,

            Amount = request.Amount,
            Narration = request.Narration,
            AccountNumber = request.SenderAccountNumber,
            BankName = request.SenderBankName,
            AccountName = request.SenderAccountName,
            BankNubanCode = request.SenderBankNubanCode,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            Longitude = request.Longitude,
            Latitude = request.Latitude,


            TransactionType = transactionType,
            TransactionChannel = transactionChannel,
            TransactionCategory = transactionCategory,
            TransactionStatus = transactionStatus,
            TransactionStatusLogs = []
        };
        var log = TransactionStatusLog.Create(txn, transactionStatus, "Initiated");
        txn.TransactionStatusLogs.Add(log);
        return txn;
    }

}
