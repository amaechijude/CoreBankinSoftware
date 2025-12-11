using CustomerAPI.Entities.Enums;

namespace CustomerAPI.Entities;

public sealed class Address : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public AddressType AddressType { get; set; }

    // Address Details
    public string BuildingNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string LocalGovernmentArea { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    // Address Verification
    public bool IsVerified { get; set; } = false;
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; } // UserProfile or system that verified the address
    public VerificationMethod VerificationMethod { get; set; } = VerificationMethod.Automated;
    public string? VerificationReference { get; set; } // Reference ID for the verification process

    // Geo-location
    public double? Latitude { get; set; } // Latitude for geo-location
    public double? Longitude { get; set; } // Longitude for geo-location

    // Helper properties
    public string FullAddress => $"{BuildingNumber} {Street}, {City}";
}
