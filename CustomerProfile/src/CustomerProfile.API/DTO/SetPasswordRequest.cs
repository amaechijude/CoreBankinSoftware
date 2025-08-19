using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace CustomerAPI.DTO
{
    public class SetPasswordRequest
    {
        [Required]
        public string? Password { get; set; }
        [Required, Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
        [Required]
        public string? Otp { get; set; }
    }

    public class SetPasswordRequestValidator : AbstractValidator<SetPasswordRequest>
    {
        public SetPasswordRequestValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$")
                    .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password);

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .MinimumLength(6).WithMessage("OTP must be at least 6 characters long.");
        }
    }
}
