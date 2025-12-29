using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace CustomerProfile.DTO
{
    public record Jwt(string Token, DateTime? ExpiresIn);
    public sealed record UserProfileResponse(
        Guid Id,
        string Username,
        string Email,
        string PhoneNumber,
        string? ImageUrl,
        Jwt Jwt
    );

    public sealed class SetProfileRequest
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;
        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty; // Lowercase, Uppercase, Number, Special Character
        [Required, Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
    }

    public sealed class SetDetailsRequestValidator : AbstractValidator<SetProfileRequest>
    {
        public SetDetailsRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters long");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");
        }
    }
}
