using FluentValidation;
namespace TransactionService.DTOs;

public record FundCreditTransferRequest
(
    string SenderAccountNumber,
    string SenderBankName,
    string SenderBankNubanCode,
    string DestinationAccountNumber,
    string DestinationBankName,
    string DestinationBankNubanCode,
    decimal Amount,
    string? Narration
);

public record FundCreditTransferResponse
(
    decimal Amount,
    string Status,
    DateTime TransactionDateTime,

    string SenderAccountNumber,
    string SenderBankName,
    string SenderAccountName,

    string BeneficiaryAccountNumber,
    string BeneficiaryBankName,
    string BeneficiaryAccountName,

    string Narration,
    string SessionID,
    string TransactionReference
);

// public record BankInfoDetails 
// (
//     string AccountNumber,
//     string AccountName,
//     string BankName
// );
public class FundCreditTransferValidator : AbstractValidator<FundCreditTransferRequest>
{
    public FundCreditTransferValidator()
    {
        RuleFor(x => x.SenderAccountNumber)
            .NotEmpty().WithMessage("SenderAccountNumber is required.")
            .Length(10).WithMessage("SenderAccountNumber must be 10 characters long.")
            .Must(IsAllDigit).WithMessage("SenderAccountNumber must contain only digits.");
        RuleFor(x => x.SenderBankName)
            .NotEmpty().WithMessage("SenderBankName is required.")
            .MinimumLength(3).WithMessage("SenderBankName must be at least 3 characters long.");
        RuleFor(x => x.SenderBankNubanCode)
            .NotEmpty().WithMessage("SenderBankNubanCode is required.")
            .Must(IsAllDigit).WithMessage("SenderBankNubanCode must contain only digits.");
        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty().WithMessage("DestinationAccountNumber is required.")
            .Length(10).WithMessage("DestinationAccountNumber must be 10 characters long.")
            .Must(IsAllDigit).WithMessage("DestinationAccountNumber must contain only digits.");
        RuleFor(x => x.DestinationBankName)
            .NotEmpty().WithMessage("DestinationBankName is required.")
            .MinimumLength(3).WithMessage("DestinationBankName must be at least 3 characters long.");
        RuleFor(x => x.DestinationBankNubanCode)
            .NotEmpty().WithMessage("DestinationBankNubanCode is required.")
            .Must(IsAllDigit).WithMessage("DestinationBankNubanCode must contain only digits.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Narration)
            .MaximumLength(100).WithMessage("Narration cannot exceed 100 characters.");
    }
    private bool IsAllDigit(string accountNumber) => accountNumber.All(char.IsDigit);
}