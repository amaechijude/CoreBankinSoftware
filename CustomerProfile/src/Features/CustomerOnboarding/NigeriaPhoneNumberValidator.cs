using System.ComponentModel.DataAnnotations;

namespace src.Features.CustomerOnboarding
{
    public class CustomPhoneNumberValidator : ValidationAttribute
    {
        public CustomPhoneNumberValidator()
        {
            ErrorMessage = "Invalid phone number format";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? input = value as string;

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult("Phone number is required.");

            // clean up the input
            input = input.Trim().Replace(" ", "").Replace("-", "");

            // Normalize the phone number to a standard format
            if (input.Length == 11 && input.StartsWith('0'))
                input = string.Concat("+234", input.AsSpan(1)); // Remove the leading zero

            if (input.Length == 13 && input.StartsWith("234"))
                input = string.Concat("+", input); // Add the plus sign

            if (input.Length == 10 && char.IsDigit(input[0]) && (int)input[0] > 0 )
                input = string.Concat("+234", input.AsSpan()); // Convert to international format

            // Validate the phone number format
            if (input.Length == 14 && input.StartsWith("+234") && long.TryParse(input.AsSpan(4), out _))
            {
                return ValidationResult.Success; // Valid Nigerian phone number
            }

            return new ValidationResult("Invalid Nigerian phone number format.");
        }
    }
}
