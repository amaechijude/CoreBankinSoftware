using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace CustomerAPI.DTO
{

    public record Address(string Street, string City, string LGA, string State);
    public class SetDetailsRequest
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;
        [Required, MinLength(11)]
        public string BVN { get; set; } = string.Empty; // 11 digits only
        [Required]
        public string Password { get; set; } = string.Empty; // 6  Digits only
        [Required, Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
        [Required]
        public Address? Address { get; set; }
    }

    public class SetDetailsRequestValidator : AbstractValidator<SetDetailsRequest>
    {
        public SetDetailsRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters long");

            RuleFor(x => x.BVN)
                .NotEmpty()
                .Length(11)
                .Matches("^[0-9]+$")
                .WithMessage("BVN must be exactly 11 digits");

            RuleFor(x => x.Password)
                .NotEmpty()
                .Length(6)
                .Matches("^[0-9]+$")
                .WithMessage("Password must be exactly 6 digits");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");

            RuleFor(x => x.Address)
                .NotNull()
                .Must(address => !string.IsNullOrEmpty(address?.Street))
                .WithMessage("Street is required")
                .Must(address => !string.IsNullOrEmpty(address?.City))
                .WithMessage("City is required")
                .Must(address => !string.IsNullOrEmpty(address?.LGA))
                .WithMessage("LGA is required")
                .Must(address => !string.IsNullOrEmpty(address?.State))
                .WithMessage("State is required");
        }
    }
}
