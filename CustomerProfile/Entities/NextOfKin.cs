using CustomerAPI.Entities.Enums;

namespace CustomerAPI.Entities
{
    public class NextOfKin : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public UserProfile? Customer { get; set; }

        // Personal Information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;

        // Contact Information
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AlternatePhoneNumber { get; set; } = string.Empty;
        public bool CanContactInEmergency { get; set; } = true;

        // Address Informatiion
        public string Address { get; set; } = string.Empty;

        // Relationship
        public NextOfKinRelationship Relationship { get; set; }
        public NextOfKinCategory Category { get; set; } = NextOfKinCategory.Primary;

        // Helper properties
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayName => $"{FullName} ({Relationship})";
        public string ContactInfo => $"{PhoneNumber} | {Email}";
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    }
}