namespace TransactionService.Entity;

public sealed class UserNotificationPreference
{
    public Guid Id { get; private init; }
    public Guid CustomerId { get; private init; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string FirstName { get; private init; } = string.Empty;
    public string LastName { get; private init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private init; }
    public string FullName => $"{FirstName} {LastName}";

    public static UserNotificationPreference Create(PreferenceRequestResponseBody request)
    {
        return new UserNotificationPreference
        {
            Id = Guid.CreateVersion7(),
            CustomerId = request.CustomerId,
            AccountNumber = request.AccountNumber,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}

public record PreferenceRequestResponseBody(
    Guid CustomerId,
    string Email,
    string PhoneNumber,
    string AccountNumber,
    string FirstName,
    string LastName
);
