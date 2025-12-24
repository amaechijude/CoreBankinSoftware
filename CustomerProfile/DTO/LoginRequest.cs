using FluentValidation;

namespace CustomerProfile.DTO
{
    public record LoginRequest(string UsernameOrPhone, string Password);
    public record ForgotPasswordRequest(string PhoneNumber);

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.PhoneNumber.Trim())
                .NotEmpty().WithMessage("Phone number is required.")
                .Length(11).WithMessage("Phone number lenght Must be 11")
                .Must(IsAllDigit).WithMessage("Phone number must contain only digits.");
        }
        private bool IsAllDigit(string phoneNumber) => phoneNumber.All(char.IsDigit);
    }

    public record ResetPasswordRequest(string OtpCode, string NewPassword, string ConfirmPassword);

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.OtpCode.Trim())
                .NotEmpty().WithMessage("OTP code is required.")
                .MaximumLength(7).WithMessage("OTP code must not be more than 7 characters long.")
                .Must(IsAllDigit).WithMessage("OTP code must contain only digits.");
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Confirm password must match the new password.");
        }
        private bool IsAllDigit(string otpCode) => otpCode.All(char.IsDigit);
    }
}
