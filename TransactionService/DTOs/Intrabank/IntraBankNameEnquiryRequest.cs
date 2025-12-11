using FluentValidation;

namespace TransactionService.DTOs.Intrabank;

public record IntraBankNameEnquiryRequest(string AccountNumber);

public record IntraBankNameEnquiryResponse(
    string AccountNuber,
    string AccountName,
    string BankName,
    double? AccountBalance
);

public class IntraBankNameEnquiryRequestValidator : AbstractValidator<IntraBankNameEnquiryRequest>
{
    public IntraBankNameEnquiryRequestValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account Number must not be empty")
            .Must(IsAllDigit)
            .WithMessage("Account Number must be digits only")
            .Length(10)
            .WithMessage("Account number must be 10 digits");
    }

    private bool IsAllDigit(string accountNumber) => accountNumber.All(char.IsDigit);
}
