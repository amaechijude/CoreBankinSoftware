using CustomerProfile.Global;

namespace CustomerProfile.Entities;

public sealed class VerificationCode
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string UserPhoneNumber { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; } = false;
    public bool CanSetProfile { get; private set; } = false;
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    private const int _expiryDurationInMinutes = 10;
    public readonly string ExpiryDuration = $"{_expiryDurationInMinutes} minutes";

    // Factory method to create
    public static VerificationCode CreateNew(string phoneNumber)
    {
        return new VerificationCode
        {
            UserPhoneNumber = phoneNumber,
            Code = GlobalUtils.GenerateVerificationCode(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes),
        };
    }

    public void MarkVerifiedAndCanSetProfile()
    {
        IsUsed = true;
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        CanSetProfile = true;
    }

    public void UpdateCode()
    {
        Code = GlobalUtils.GenerateVerificationCode();
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryDurationInMinutes);
        IsUsed = false;
        CanSetProfile = false;
    }
}
