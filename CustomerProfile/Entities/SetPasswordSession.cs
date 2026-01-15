using System.ComponentModel.DataAnnotations;

namespace CustomerProfile.Entities;

public sealed class SetPasswordSession
{
    public Guid Id { get; private init; }

    [MaxLength(11)]
    public string PhoneNumber { get; private set; } = string.Empty;

    public static SetPasswordSession Create(string phoneNumber)
    {
        return new SetPasswordSession { Id = Guid.CreateVersion7(), PhoneNumber = phoneNumber };
    }
}
