using FluentValidation;
using SharedGrpcContracts.Protos.Account.Operations.V1;

namespace AccountServices.Validators;

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.CustomerId).Must(IsValidGuid).WithMessage("Not a valid Guid");

        RuleFor(x => x.PhoneNumber)
            .Length(11)
            .WithMessage("Phone number must be 11 digits")
            .Must(IsAllDigits)
            .WithMessage("Phone number must be 11 digits");

        RuleFor(x => x.AccountName)
            .NotEmpty()
            .WithMessage("account name cannot be empty")
            .MinimumLength(4)
            .WithMessage("Limited characters");
    }

    private bool IsValidGuid(string customerId) => Guid.TryParse(customerId, out _);

    private bool IsAllDigits(string phoneNumber) => phoneNumber.All(char.IsDigit);
}
