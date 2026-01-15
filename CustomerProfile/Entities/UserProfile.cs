using CustomerProfile.DTO.BvnNinVerification;
using CustomerProfile.Entities.Enums;
using CustomerProfile.Entities.ValueObjects;

namespace CustomerProfile.Entities;

public sealed class UserProfile
{
    public Guid Id { get; private init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Address> Addresses { get; private set; } = [];
    public ICollection<NextOfKin> NextOfKins { get; private set; } = [];
    public ICollection<KYCDocument> KycDocuments { get; private set; } = [];
    public ICollection<ComplianceCheck> ComplianceChecks { get; private set; } = [];
    public ICollection<RiskAssessment> RiskAssessments { get; private set; } = [];
    public ICollection<NotificationChannels> NotificationChannels { get; private set; } = [];

    // Enums
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public CustomerStatus Status { get; set; } = CustomerStatus.PendingActivation;
    public KYCStatus KYCStatus { get; private set; } = KYCStatus.NotStarted;
    public AccountTier AccountTier { get; set; } = AccountTier.Tier1;
    public EmployementType EmploymentType { get; set; } = EmployementType.SelfEmployed;
    public CustomerSource Source { get; set; } = CustomerSource.Online;

    // Personal Information
    public string? UserAccountNumber { get; private set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? MaidenName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? StateOfOrigin { get; set; }
    public string? Nationality { get; set; }
    public string? Title { get; set; }
    public MaritalStatus MaritalStatus { get; set; }

    // Contact Information
    public string PasswordHash { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? AlternateEmail { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string InternationalPhoneNumber { get; private set; } = string.Empty;
    public string? AlternatePhoneNumber { get; private set; }
    public OnboardingStage OnboardingStage { get; private set; } = OnboardingStage.VerifiedPhoneOtp;

    // Refresh Token
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? RefreshTokenExpires { get; private set; }
    public DateTimeOffset? RefreshTokenRevoked { get; private set; }
    public bool IsRefreshTokenRevoked => RefreshTokenRevoked is not null;
    public bool IsRefreshTokenExpired =>
        RefreshTokenExpires is not null && RefreshTokenExpires < DateTimeOffset.UtcNow;

    // Nigerian Banking Specific Identifiers
    public string BvnHash { get; private set; } = string.Empty; // Bank Verification Number
    public string NinHash { get; private set; } = string.Empty; // National Identification Number
    public BvnData? BvnData { get; private set; }
    public NinData? NinData { get; private set; }
    public DateTimeOffset? BVNAddedAt { get; private set; }
    public DateTimeOffset? NINAddedAt { get; private set; }

    // Biometrics
    public string? ImageUrl { get; private set; } // URL to the customer's image
    public string? NinBase64Image { get; private set; }
    public string? BvnBase64Image { get; private set; }
    public float[]? FaceEncodings { get; private set; } // Array of floats representing face encoding
    public DateTimeOffset? LastTransactionDate { get; private set; }

    // Compliance and Risk
    public bool IsPoliticallyExposedPerson { get; private set; } = false;
    public DateTimeOffset? PoliticallyExposedPersonSince { get; private set; }
    public bool IsWatchlisted { get; private set; } = false;
    public DateTimeOffset? WatchlistScreenedAt { get; private set; }
    public DateTimeOffset? WatchlistScreenedUntil { get; private set; }
    public DateTimeOffset? LastAMLScreening { get; private set; }

    // Helper Properties
    public string FullName => $"{FirstName} {MiddleName} {LastName}";
    public int Age =>
        DateTime.Now.Year
        - DateOfBirth.Year
        - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public bool IsMinor => Age < 18;
    public bool IsDormant =>
        LastTransactionDate.HasValue
        && LastTransactionDate.Value < DateTimeOffset.Now.AddDays(-365);

    // Factory methods to create user
    public static UserProfile CreateNewUser(string phoneNumber)
    {
        if (
            string.IsNullOrWhiteSpace(phoneNumber)
            || !phoneNumber.All(char.IsDigit)
            || phoneNumber.Length != 11
        )
        {
            throw new InvalidPhoneNumberException("Invalid phone number");
        }
        return new UserProfile
        {
            Id = Guid.CreateVersion7(),
            PhoneNumber = phoneNumber,
            UserAccountNumber = phoneNumber[1..],
            InternationalPhoneNumber = "+234" + phoneNumber[1..],
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void SetRefreshToken(string token)
    {
        RefreshToken = token;
        RefreshTokenExpires = DateTimeOffset.UtcNow.AddDays(7);
        RefreshTokenRevoked = null;
    }

    public void UpdateRefreshToken(string token)
    {
        SetRefreshToken(token);
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenRevoked = DateTimeOffset.UtcNow;
    }

    public void AddPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    // Methods to update sensitive information
    public void AddBvn(BvnApiResponse response)
    {
        if (BvnData is not null || !string.IsNullOrWhiteSpace(BvnHash))
        {
            return; // BVN already added, do nothing
        }

        var bvnData = BvnData.Create(this, response);
        if (bvnData is null)
        {
            return;
        }

        BvnData = bvnData;
        BvnBase64Image = response.Data?.Base64Image;
        BVNAddedAt = DateTimeOffset.UtcNow;
        return;
    }

    public void RemoveBvn()
    {
        if (BvnData is null && string.IsNullOrWhiteSpace(BvnHash))
        {
            return; // BVN not set, do nothing
        }

        BvnData = null;
        BvnHash = string.Empty;
        BvnBase64Image = null;
        BVNAddedAt = null;
        return;
    }

    public bool BvnExists => BvnData is not null && !string.IsNullOrWhiteSpace(BvnBase64Image);
}

internal sealed class InvalidPhoneNumberException(string message) : Exception(message);

internal sealed class InvalidEmailException(string message) : Exception(message);
