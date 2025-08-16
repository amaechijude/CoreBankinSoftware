namespace CustomerAPI.Entities.ValueObjects
{
    public class NIN(string value)
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


        private static string ValidatedNIN(string value)
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
}