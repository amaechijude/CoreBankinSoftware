using CustomerProfile.DTO.BvnNinVerification;

namespace CustomerProfile.Entities.ValueObjects;

public class BVN(string value) : IEquatable<BVN>
{
    public string Value { get; } = ValidatedBVN(value);

    // Implicit conversion methods
    public static implicit operator BVN(string value) => new(value);

    public static implicit operator string(BVN bvn) => bvn.Value;

    // IEquatable implementation
    public bool Equals(BVN? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as BVN);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    private static string ValidatedBVN(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("BVN cannot be null or empty.", nameof(value));
        }

        value = value.Trim();

        if (value.Length != 11)
        {
            throw new ArgumentException("BVN must be exactly 11 digits long.", nameof(value));
        }

        if (value.All(char.IsDigit) == false)
        {
            throw new ArgumentException("BVN must contain only digits.", nameof(value));
        }

        return value;
    }
}

public class BvnData : BaseEntity
{
    public Guid UserProfileId { get; private init; }
    public UserProfile UserProfile { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Gender { get; private set; } = string.Empty; // F or M
    public string MaritalStatus { get; private set; } = string.Empty;
    public string WatchListed { get; private set; } = string.Empty; // YES or NO
    public string LevelOfAccount { get; private set; } = string.Empty;
    public string BVN { get; private init; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public string PhoneNumber1 { get; private set; } = string.Empty;
    public string PhoneNumber2 { get; private set; } = string.Empty;
    public DateOnly? RegistrationDate { get; private set; }
    public string EnrollmentBank { get; private init; } = string.Empty;
    public string EnrollmentBranch { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string LgaOfOrigin { get; private set; } = string.Empty;
    public string LgaOfResidence { get; private set; } = string.Empty;
    public string Nin { get; private set; } = string.Empty;
    public string NameOnCard { get; private set; } = string.Empty;
    public string Nationality { get; private set; } = string.Empty;
    public string ResidentialAddress { get; private set; } = string.Empty;
    public string StateOfOrigin { get; private set; } = string.Empty;
    public string StateOfResidence { get; private set; } = string.Empty;
    public string Base64Image { get; private set; } = string.Empty;

    // Factory method to create
    public static BvnData? Create(UserProfile user, BvnApiResponse response)
    {
        if (response?.Data is null)
        {
            return null;
        }

        ;

        return new BvnData
        {
            UserProfileId = user.Id,
            UserProfile = user,
            Title = response.Data.Title ?? string.Empty,
            Gender = response.Data.Gender ?? string.Empty,
            MaritalStatus = response.Data.MaritalStatus ?? string.Empty,
            WatchListed = response.Data.WatchListed ?? string.Empty,
            LevelOfAccount = response.Data.LevelOfAccount ?? string.Empty,
            BVN = response.Data.Bvn ?? string.Empty,
            FirstName = response.Data.FirstName ?? string.Empty,
            MiddleName = response.Data.MiddleName ?? string.Empty,
            LastName = response.Data.LastName ?? string.Empty,
            DateOfBirth = response.Data.DateOfBirth,
            PhoneNumber1 = response.Data.PhoneNumber1 ?? string.Empty,
            PhoneNumber2 = response.Data.PhoneNumber2 ?? string.Empty,
            RegistrationDate = response.Data.RegistrationDate,
            EnrollmentBank = response.Data.EnrollmentBank ?? string.Empty,
            EnrollmentBranch = response.Data.EnrollmentBranch ?? string.Empty,
            Email = response.Data.Email ?? string.Empty,
            LgaOfOrigin = response.Data.LgaOfOrigin ?? string.Empty,
            LgaOfResidence = response.Data.LgaOfResidence ?? string.Empty,
            Nin = response.Data.Nin ?? string.Empty,
            NameOnCard = response.Data.NameOnCard ?? string.Empty,
            Nationality = response.Data.Nationality ?? string.Empty,
            ResidentialAddress = response.Data.ResidentialAddress ?? string.Empty,
            StateOfOrigin = response.Data.StateOfOrigin ?? string.Empty,
            StateOfResidence = response.Data.StateOfResidence ?? string.Empty,
            Base64Image = response.Data.Base64Image ?? string.Empty,
        };
    }
}
