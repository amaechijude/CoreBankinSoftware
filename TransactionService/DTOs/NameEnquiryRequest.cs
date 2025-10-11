using FluentValidation;

namespace TransactionService.DTOs;

public record NameEnquiryRequest
(
    string SenderAccountNumber,
    string SenderBankName,
    string SenderBankNubanCode,
    string DestinationAccountNumber,
    string DestinationBankName,
    string DestinationBankNubanCode
);

public record NameEnquiryResponse
(
    string AccountNumber,
    string AccountName,
    string BankCode,
    string BankName
);

public class NameEnquiryValidator : AbstractValidator<NameEnquiryRequest>
{
    public NameEnquiryValidator()
    {
        RuleFor(x => x.SenderAccountNumber)
            .NotEmpty().WithMessage("SenderAccountNumber is.")
            .Length(10).WithMessage("SenderAccountNumber must be 10 characters long.")
            .Must(IsAllDigit).WithMessage("SenderAccountNumber must contain only digits.");
        RuleFor(x => x.SenderBankName)
            .NotEmpty().WithMessage("SenderBankName is.")
            .MinimumLength(3).WithMessage("SenderBankName must be at least 3 characters long.");
        RuleFor(x => x.SenderBankNubanCode)
            .NotEmpty().WithMessage("SenderBankNubanCode is.")
            .Must(IsAllDigit).WithMessage("SenderBankNubanCode must contain only digits.");
        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty().WithMessage("DestinationAccountNumber is.")
            .Length(10).WithMessage("DestinationAccountNumber must be 10 characters long.")
            .Must(IsAllDigit).WithMessage("DestinationAccountNumber must contain only digits.");
        RuleFor(x => x.DestinationBankName)
            .NotEmpty().WithMessage("DestinationBankName is.")
            .MinimumLength(3).WithMessage("DestinationBankName must be at least 3 characters long.");
        RuleFor(x => x.DestinationBankNubanCode)
            .NotEmpty().WithMessage("DestinationBankNubanCode is.")
            .Must(IsAllDigit).WithMessage("DestinationBankNubanCode must contain only digits.");

    }
    private bool IsAllDigit(string accountNumber) => accountNumber.All(char.IsDigit);
}
