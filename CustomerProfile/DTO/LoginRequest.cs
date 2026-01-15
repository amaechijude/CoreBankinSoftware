using FluentValidation;

namespace CustomerProfile.DTO;

public sealed record LoginRequest(string PhoneNumber, string Pin);

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Length(11)
            .WithMessage("Phone number lenght Must be 11")
            .Matches("[0-9]")
            .WithMessage("Phone number must contain only digits.");

        RuleFor(x => x.Pin)
            .NotEmpty()
            .WithMessage("Password is required.")
            .Length(6)
            .WithMessage("Pin must be 6 characters long.")
            .Matches("[0-9]")
            .WithMessage("Pin number must contain only digits.");
    }
}

public record LoginResponse(string Token, string RefreshToken);

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);

public sealed record ForgotPasswordRequest(string PhoneNumber);

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.PhoneNumber.Trim())
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Length(11)
            .WithMessage("Phone number lenght Must be 11")
            .Matches("[0-9]")
            .WithMessage("Phone number must contain only digits.");
    }
}

public sealed record ResetPasswordRequest(
    string OtpCode,
    string NewPassword,
    string ConfirmPassword
);
