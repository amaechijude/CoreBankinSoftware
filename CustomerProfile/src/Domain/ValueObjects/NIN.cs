namespace src.Domain.ValueObjects
{
    /// <summary>
    /// Constructor to create a new instance of the NIN value object.
    /// </summary>
    /// <param name="value">The NIN value.</param>
    public class NIN(string value) : IEquatable<NIN>
    {
        /// <summary>
        /// NIN Value
        /// </summary>
        public string Value { get; private set; } = ValidatedNIN(value);

        /// <summary>
        /// Implicit conversion from string to NIN.
        /// </summary>
        public static implicit operator NIN(string value) => new(value);

        /// <summary>
        /// Implicit conversion from NIN to string.
        /// </summary>
        public static implicit operator string(NIN nin) => nin.Value;

        public bool Equals(NIN? other) => other is not null && Value == other.Value;
        public override bool Equals(object? obj) => Equals(obj as NIN);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;


        /// <summary>
        /// NIN Value       
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The validated NIN value.</returns>
        /// <exception cref="ArgumentException"></exception>
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