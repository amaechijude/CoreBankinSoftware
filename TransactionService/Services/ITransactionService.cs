using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Entity;
using TransactionService.Entity.Enums;

namespace TransactionService.Services;

public interface ITransactionService
{
    Task<TransactionResult> InitiateTransactionAsync(TransactionRequest request, string idempotencyKey);
    Task<TransactionResult> ProcessTransactionAsync(Guid transactionId);
    Task<Transaction> GetTransactionAsync(Guid transactionId);
    Task<TransactionResult> ReverseTransactionAsync(Guid transactionId, string reason);
}

public record TransactionRequest
(
    Decimal Amount,
    string Narration,
    string SourceAccountNumber,
    string SourceBankName,
    string SourceAccountName,
    string BeneficiaryAcountNumber,
    string BeneficiaryBankName,
    string BeneficiaryAccountName,
    TransactionType TransactionType,
    TransactionChannel TransactionChannel,
    string SessionId,
    string DeviceInfo,
    string IpAddress,
    string InitiatedBy
);
public record TransactionResult
(
    bool IsSuccess,
    string Message
);