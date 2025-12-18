using FluentValidation;

namespace TransactionService.DTOs.IntraBank;

public record TransferRequestIntra(
    bool IsIntraBank,
    string IdempotencyKey,
    Guid CustomerId,
    string SessionId,
    string SenderAccountNumber,
    string SenderAccountName,
    string DestinationAccountNumber,
    string DestinationAccountName,
    decimal Amount,
    string? Narration,
    string DeviceInfo,
    string IpAddress,
    string? Longitude,
    string? Latitude,
    string? TransactionChannel
);

public record TransferResponseIntra(
    decimal Amount,
    string Status,
    DateTimeOffset TransactionDateTime,
    string SessionID,
    string TransactionReference
);

public sealed class TransferValidator : AbstractValidator<TransferRequestIntra>
{
    public TransferValidator()
    {
        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty()
            .WithMessage("DestinationAccountNumber is required.")
            .Length(10)
            .WithMessage("DestinationAccountNumber must be 10 characters long.")
            .Must(IsAllDigit)
            .WithMessage("DestinationAccountNumber must contain only digits.");

        RuleFor(x => x.Amount).GreaterThan(50).WithMessage("Amount must be greater than 50.");
    }

    private bool IsAllDigit(string accountNumber)
    {
        return accountNumber.All(char.IsDigit);
    }
}
