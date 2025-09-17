using AccountServices.Domain.Enums;

namespace AccountServices.Domain.Entities;

public sealed class Account
{
  public Guid Id { get; private set; }
  public string AccountNumber { get; private set; } = string.Empty;
  public Guid CustomerId { get; private set; }
  public string PhoneNumber { get; private set; } = string.Empty; // Alternate Account Number
  public AccountType Type { get; private set; }
  public AccountStatus Status { get; private set; }
  public decimal Balance { get; private set; } = 0.00m;
  public bool IsOnPostNoDebit { get; private set; } = false;
  public DateTimeOffset CreatedAtUtc { get; private set; }
  public DateTimeOffset? UpdatedAtUtc { get; private set; }
  public DateTimeOffset OpenedAtUtc { get; private set; }
  public DateTimeOffset? ClosedAtUtc { get; private set; }

  public static Account Create(string accountNumber, Guid customerId, string phoneNumber, AccountType type = AccountType.Savings)
  {
    if (string.IsNullOrWhiteSpace(accountNumber))
      throw new ArgumentException("Account number is required.", nameof(accountNumber));

    return new Account
    {
      Id = Guid.CreateVersion7(),
      AccountNumber = accountNumber,
      CustomerId = customerId,
      PhoneNumber = phoneNumber,
      Type = type,
      Status = AccountStatus.Active,
      Balance = 0,
      CreatedAtUtc = DateTimeOffset.UtcNow,
    };
  }
}
