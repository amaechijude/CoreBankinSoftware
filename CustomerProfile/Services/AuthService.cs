using System.Security.Claims;
using System.Threading.Channels;
using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Messaging.SMS;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class AuthService(
    UserProfileDbContext context,
    ILogger<AuthService> logger,
    JwtTokenProviderService jwtTokenProvider,
    Channel<SendSMSCommand> smsChannel,
    IPasswordHasher<UserProfile> passwordHasher
)
{
    private readonly UserProfileDbContext _context = context;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
    private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
    private readonly IPasswordHasher<UserProfile> _passwordHasher = passwordHasher;

    public async Task<ApiResponse<LoginResponse>> HandleLoginAsync(
        LoginRequest request,
        CancellationToken ct
    )
    {
        var user = await _context.UserProfiles.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.PhoneNumber,
            cancellationToken: ct
        );

        if (user is null)
        {
            return ApiResponse<LoginResponse>.Error("Invalid credentials");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Pin
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return ApiResponse<LoginResponse>.Error("Invalid credentials");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.AddPasswordHash(_passwordHasher.HashPassword(user, request.Pin));
        }

        var (accessToken, refreshToken) = _jwtTokenProvider.GenerateUserJwtToken(user);

        user.SetRefreshToken(refreshToken);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken));
    }

    public async Task<ApiResponse<LoginResponse>> HandleRefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken ct
    )
    {
        var principal = await _jwtTokenProvider.ValidateToken(request.AccessToken);
        if (principal is null)
        {
            return ApiResponse<LoginResponse>.Error("Invalid token");
        }

        string? userId = principal.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return ApiResponse<LoginResponse>.Error("Invalid token, please login again");
        }

        var user = await _context.UserProfiles.FirstOrDefaultAsync(
            x => x.Id == guid,
            cancellationToken: ct
        );

        if (user is null || string.IsNullOrWhiteSpace(user.RefreshToken))
        {
            return ApiResponse<LoginResponse>.Error("User not found, please login again");
        }
        if (user.RefreshToken != request.RefreshToken)
        {
            return ApiResponse<LoginResponse>.Error("Invalid token, please login again");
        }
        if (user.IsRefreshTokenRevoked || user.IsRefreshTokenExpired)
        {
            return ApiResponse<LoginResponse>.Error("Invalid token, please login again");
        }

        var (accessToken, refreshToken) = _jwtTokenProvider.GenerateUserJwtToken(user);

        user.SetRefreshToken(refreshToken);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken));
    }

    public async Task<ApiResponse<OnboardingResponse>> HandleForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        var validator = new ForgotPasswordRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<OnboardingResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }
        var user = await _context.UserProfiles.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.PhoneNumber,
            cancellationToken: ct
        );

        if (user is null)
        {
            return ApiResponse<OnboardingResponse>.Error("User not found");
        }

        return await HandleOtp(user.PhoneNumber, ct);
    }

    public async Task<ApiResponse<string>> HandleResetPasswordAsync(
        Guid validId,
        ResetPasswordRequest request,
        CancellationToken ct
    )
    {
        var verificationCode = await _context
            .VerificationCodes.Where(v => v.Id == validId && v.Code == request.OtpCode)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (verificationCode is null)
        {
            return ApiResponse<string>.Error("Reset Password Failed");
        }

        if (verificationCode.IsUsed || verificationCode.IsExpired)
        {
            return ApiResponse<string>.Error("Otp Expired");
        }

        var user = await _context.UserProfiles.FirstOrDefaultAsync(
            u => u.PhoneNumber == verificationCode.UserPhoneNumber,
            cancellationToken: ct
        );
        if (user is null)
        {
            return ApiResponse<string>.Error("Password reset failed");
        }

        var passwordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.AddPasswordHash(passwordHash);

        await _context.SaveChangesAsync(ct);
        await _context
            .VerificationCodes.Where(v => v.Code == request.OtpCode)
            .ExecuteDeleteAsync(cancellationToken: ct);

        return ApiResponse<string>.Success("Password reset successful, login");
    }

    private async Task<ApiResponse<OnboardingResponse>> HandleOtp(
        string phoneNumber11digit,
        CancellationToken ct
    )
    {
        var existingCode = await _context
            .VerificationCodes.Where(v => v.UserPhoneNumber == phoneNumber11digit)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (existingCode is null)
        {
            var newCode = VerificationCode.CreateNew(phoneNumber11digit);

            await _context.VerificationCodes.AddAsync(newCode, ct);
            var token = _jwtTokenProvider.GenerateVerificationResponseJwtToken(newCode);
            await EnqueueSms(phoneNumber11digit, newCode.Code, ct);
            await _context.SaveChangesAsync(ct);
            return ApiResponse<OnboardingResponse>.Success(new OnboardingResponse(token, null));
        }
        existingCode.UpdateCode();
        await _context.SaveChangesAsync(ct);
        var tokn = _jwtTokenProvider.GenerateVerificationResponseJwtToken(existingCode);
        await EnqueueSms(existingCode.UserPhoneNumber, existingCode.Code, ct);
        return ApiResponse<OnboardingResponse>.Success(new OnboardingResponse(tokn, null));
    }

    private async Task EnqueueSms(string phoneNumber, string code, CancellationToken ct)
    {
        var message = $"Your Otp is {code} ";
        var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
        await _smsChannel.Writer.WriteAsync(sendSmsCommand, ct);
    }
}
