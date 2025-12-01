using FluentValidation;

namespace TransactionService.DTOs.Intrabank;

public record IntraBankTransferResponse(string Message, string Status);

public record IntraBankTransferRequest(Guid CustomerId, string AccountNumber, decimal Amount);

public class IntraBankTransferRequestValidator : AbstractValidator<IntraBankTransferRequest>
{
    public IntraBankTransferRequestValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account Number must not be empty")
            .Must(IsAllDigit)
            .WithMessage("Account Number must be digits only")
            .Length(10)
            .WithMessage("Account number must be 10 digits");

        RuleFor(x => x.Amount).GreaterThan(50);
    }
    private bool IsAllDigit(string accountNumber) => accountNumber.All(char.IsDigit);
}