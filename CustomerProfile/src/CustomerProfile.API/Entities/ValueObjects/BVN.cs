namespace CustomerAPI.Entities.ValueObjects
{
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
                throw new ArgumentException("BVN cannot be null or empty.", nameof(value));

            value = value.Trim();

            if (value.Length != 11)
                throw new ArgumentException("BVN must be exactly 11 digits long.", nameof(value));

            if (value.All(char.IsDigit) == false)
                throw new ArgumentException("BVN must contain only digits.", nameof(value));

            return value;
        }

    }

    public class BvnData : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? Title { get; set; }
        public string? Gender { get; set; } // F or M
        public string? MaritalStatus { get; set; }
        public string? WatchListed { get; set; } // YES or NO
        public string? LevelOfAccount { get; set; }
        public string? BVN { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? RegistrationDate { get; set; }
        public string? EnrollmentBank { get; set; }
        public string? EnrollmentBranch { get; set; }
        public string? Email { get; set; }
        public string? LgaOfOrigin { get; set; }
        public string? LgaOfResidence { get; set; }
        public string? Nin { get; set; }
        public string? NameOnCard { get; set; }
        public string? Nationality { get; set; }
        public string? ResidentialAddress { get; set; }
        public string? StateOfOrigin { get; set; }
        public string? StateOfResidence { get; set; }
        public string? Base64Image { get; set; }
    }
}
