using System.ComponentModel.DataAnnotations;
using CustomerProfile.Entities.Enums;

namespace CustomerProfile.DTO;

public record AddNextOfKinRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? MiddleName,
    [Required] DateOnly DateOfBirth,
    [Required] Gender Gender,
    [Required] string Nationality,
    [Required] string Occupation,
    [Required] string Email,
    [Required] string PhoneNumber,
    string? AlternatePhoneNumber,
    [Required] string Address,
    [Required] NextOfKinRelationship Relationship,
    NextOfKinCategory Category = NextOfKinCategory.Primary,
    bool CanContactInEmergency = true
);

public record NextOfKinResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Relationship,
    string PhoneNumber,
    string Email,
    string Address
);
