namespace TransactionService.DTOs;

public class InitiateTransactionRequest
{
    public required decimal Amount { get; set; }
    public required string ReceiverAccountNumber { get; set; }
    public required string ReceiverBankCode { get; set; }
    public string? Narration { get; set; }
}
