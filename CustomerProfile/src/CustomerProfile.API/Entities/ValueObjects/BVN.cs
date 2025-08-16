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
}
