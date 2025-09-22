using FluentValidation;
using SharedGrpcContracts.Protos.Account.V1;

namespace AccountServices.Services
{
    public class AccountRequestValidators : AbstractValidator<CreateAccountRequest>
    {
        public AccountRequestValidators()
        {
            RuleFor(x => x.CustomerId.Trim())
                .NotEmpty().WithMessage("Account number is required.")
                .Must(IsValidGuid).WithMessage("Account number must be a valid GUID.");

            RuleFor(x => x.PhoneNumber.Trim())
                .NotEmpty().WithMessage("Phone number is required.")
                .Length(11).WithMessage("Phone number lenght Must be 11")
                .Must(IsAllDigit).WithMessage("phone number not all digits")
                .Must(StartsWithZero).WithMessage("Phone number must start with zero");
        }

        private bool IsValidGuid(string id) => Guid.TryParse(id, out Guid _);
        private bool IsAllDigit(string phone) => phone.All(char.IsDigit);
        private bool StartsWithZero(string phone) => phone.StartsWith('0');
    }
}