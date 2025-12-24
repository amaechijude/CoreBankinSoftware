using CustomerProfile.DTO.BvnNinVerification;

namespace CustomerProfile.Entities.ValueObjects
{
    public class NIN(string? value)
    {
        public string Value { get; private set; } = ValidatedNIN(value);

        public static implicit operator NIN(string value) => new(value);

        /// <summary>
        /// Implicit conversion from NIN to string.
        /// </summary>
        public static implicit operator string(NIN nin) => nin.Value;

        public bool Equals(NIN? other) => other is not null && Value == other.Value;
        public override bool Equals(object? obj) => Equals(obj as NIN);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;


        private static string ValidatedNIN(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("NIN cannot be null or empty.", nameof(value));

            value = value.Trim();

            if (value.Length != 11)
                throw new ArgumentException("NIN must be exactly 11 characters long.", nameof(value));

            if (value.All(char.IsDigit) == false)
                throw new ArgumentException("NIN must contain only digits.", nameof(value));

            return value;
        }
    }

    public class NinData : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public UserProfile? Customer { get; set; }
        public string? Birthdate { get; set; }
        public string? Email { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? FirstName { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? MiddleName { get; set; }
        public string? Nin { get; set; }
        public string? Photo { get; set; }
        public string? Profession { get; set; }
        public string? Religion { get; set; }
        public string? ResidenceAddressLine1 { get; set; }
        public string? ResidenceTown { get; set; }
        public string? ResidenceLga { get; set; }
        public string? ResidenceState { get; set; }
        public string? ResidenceStatus { get; set; }
        public string? Signature { get; set; }
        public string? Surname { get; set; }
        public string? TelephoneNo { get; set; }
        public string? Title { get; set; }
        public string? TrackingId { get; set; }

        internal NinData? Create(UserProfile userProfile, NINAPIResponse response)
        {
            throw new NotImplementedException();
        }
    }
}