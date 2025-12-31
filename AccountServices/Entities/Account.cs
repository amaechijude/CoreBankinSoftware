using AccountServices.Entities.Enums;

namespace AccountServices.Entities;

public sealed class Account
{
    public Guid Id { get; private init; }
    public Guid CustomerId { get; private init; }
    public string AccountNumber { get; private init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public AccountStatus Status { get; private set; }
    public decimal Balance { get; private set; }
    public decimal ReservedAmount { get; private set; }
    public uint RowVersion { get; set; }
    public bool IsOnPostNoDebit { get; private set; } = false;
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public string? AccountName { get; private set; }
    public string? BankName { get; set; }

    public static Account Create(
        Guid customerId,
        string phoneNumber,
        AccountType accountType,
        string accountName
    )
    {
        return new Account
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            AccountNumber = PhoneToAccountNumber(phoneNumber),
            PhoneNumber = phoneNumber,
            Status = AccountStatus.Active,
            Balance = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            AccountType = accountType,
            BankName = "HeartBeat",
            AccountName = accountName,
        };
    }

    private static string PhoneToAccountNumber(string phone) => phone[1..]; // 10-digit account number

    public void DebitAccount(decimal amount)
    {
        if ((Balance - ReservedAmount) < amount)
        {
            throw new InsufficientBalanceException("Insufficient balance");
        }

        Balance -= amount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void CreditAccount(decimal amount)
    {
        Balance += amount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal bool IsInsufficient(decimal amount) => (Balance - ReservedAmount) < amount;
}

internal sealed class InsufficientBalanceException(string message) : Exception(message);
