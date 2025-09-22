using AccountServices.Domain.Enums;

namespace AccountServices.Domain.Entities;

public sealed class Account
{
  public Guid Id { get; private set; }
  public Guid CustomerId { get; private set; }
  public string PhoneAccountNumber { get; private set; } = string.Empty; // phone number Account Number
  public AccountType Type { get; private set; }
  public AccountStatus Status { get; private set; }
  public decimal Balance { get; private set; } = 0.00m;
  public bool IsOnPostNoDebit { get; private set; } = false;
  public DateTimeOffset CreatedAtUtc { get; private set; }
  public DateTimeOffset? UpdatedAtUtc { get; private set; }
  public DateTimeOffset OpenedAtUtc { get; private set; }
  public DateTimeOffset? ClosedAtUtc { get; private set; }

  public static Account Create(Guid customerId, string phoneNumber)
  {
    return new Account
    {
      Id = Guid.CreateVersion7(),
      CustomerId = customerId,
      PhoneAccountNumber = PhoneToAccountNumber(phoneNumber),
      Type = AccountType.Savings,
      Status = AccountStatus.Active,
      Balance = 0,
      CreatedAtUtc = DateTimeOffset.UtcNow,
    };
  }

  private static string PhoneToAccountNumber(string phone)
  {
    phone = phone.Trim();
    if (!phone.All(char.IsDigit))
      throw new ArgumentException("All is not digit");

    if (phone.Length != 11)
      throw new ArgumentException("Invalid phone number");

    if (!phone.StartsWith('0'))
      throw new ArgumentException("phone number did not start with 0");

    return phone[1..]; // 10 digit account number
  }
}
