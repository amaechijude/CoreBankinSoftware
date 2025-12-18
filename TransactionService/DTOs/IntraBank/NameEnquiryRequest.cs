using FluentValidation;

namespace TransactionService.DTOs.IntraBank;

public record NameEnquiryIntraRequest(string AccountNumber);

public record NameEnquiryIntraResponse(
    string AccountNumber,
    string AccountName,
    string BankCode,
    string BankName
);

public class NameEnquiryIntraValidator : AbstractValidator<NameEnquiryIntraRequest>
{
    public NameEnquiryIntraValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("AccountNumber is required.")
            .Length(10)
            .WithMessage("AccountNumber must be 10 characters long.")
            .Must(IsAllDigit)
            .WithMessage("AccountNumber must contain only digits.");
    }

    private bool IsAllDigit(string accountNumber)
    {
        return accountNumber.All(char.IsDigit);
    }
}
