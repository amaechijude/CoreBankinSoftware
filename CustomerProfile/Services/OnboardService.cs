using System.Security.Claims;
using System.Threading.Channels;
using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Messaging.SMS;
using FluentValidation;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class OnboardService(
    UserProfileDbContext context,
    ILogger<OnboardService> logger,
    JwtTokenProviderService jwtTokenProvider,
    Channel<SendSMSCommand> smsChannel,
    IValidator<OnboardingRequest> onboardingValidator,
    IValidator<SetSixDigitPinRequest> setSixDigitPinValidator,
    IPasswordHasher<UserProfile> passwordHasher
)
{
    private readonly UserProfileDbContext _context = context;
    private readonly ILogger<OnboardService> _logger = logger;
    private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
    private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
    private readonly IValidator<OnboardingRequest> _onboardingValidator = onboardingValidator;
    private readonly IValidator<SetSixDigitPinRequest> _setSixDigitPinValidator =
        setSixDigitPinValidator;

    private readonly IPasswordHasher<UserProfile> _passwordHasher = passwordHasher;

    public async Task<ApiResponse<OnboardingResponse>> InitiateOnboard(
        OnboardingRequest command,
        CancellationToken ct
    )
    {
        var validationResult = await _onboardingValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<OnboardingResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }

        var user = await _context
            .UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.PhoneNumber == command.PhoneNumber, cancellationToken: ct);
        if (user is not null)
        {
            var message = "Someone tried to sign up with your phone number";
            await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber, message), ct);
            return ApiResponse<OnboardingResponse>.Error(
                "Phone Number already registered, Try Login"
            );
        }
        var response = await HandleOtp(command.PhoneNumber, ct);
        return response;
    }

    public async Task<ApiResponse<string>> VerifyOtpAsync(
        Guid vId,
        OtpVerifyRequestBody request,
        CancellationToken ct
    )
    {
        var verificationCode = await _context
            .VerificationCodes.Where(v => v.Id == vId && v.Code == request.OtpCode)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (verificationCode is null)
        {
            return ApiResponse<string>.Error("Verification Failed");
        }

        if (verificationCode.IsUsed || verificationCode.IsExpired)
        {
            return ApiResponse<string>.Error("Verification Expired");
        }

        verificationCode.MarkVerifiedAndCanSetProfile();
        await _context.SaveChangesAsync(ct);

        return ApiResponse<string>.Success("Success");
    }

    public async Task<ApiResponse<string>> SetSixDigitPinAsync(
        Guid vId,
        SetSixDigitPinRequest request,
        CancellationToken ct
    )
    {
        var validationResult = await _setSixDigitPinValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<string>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        var verificationCode = await _context
            .VerificationCodes.Where(v => v.Id == vId)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (verificationCode is null)
        {
            return ApiResponse<string>.Error("Session expired or invalid");
        }
        if (!verificationCode.CanSetProfile)
        {
            return ApiResponse<string>.Error("Verification code not verified");
        }

        // Ensure user does not already exist to prevent duplicate key exceptions
        var userExists = await _context.UserProfiles.AnyAsync(
            u => u.PhoneNumber == verificationCode.UserPhoneNumber,
            cancellationToken: ct
        );

        if (userExists)
        {
            return ApiResponse<string>.Error("User already registered. Please login.");
        }

        var user = UserProfile.CreateNewUser(verificationCode.UserPhoneNumber);
        var passwordHash = _passwordHasher.HashPassword(user, request.Pin);
        user.AddPasswordHash(passwordHash);
        _context.UserProfiles.Add(user);

        _context.VerificationCodes.Remove(verificationCode);

        await _context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return ApiResponse<string>.Success("Success");
    }

    public async Task<ApiResponse<UserProfileResponse>> HandleLoginAsync(
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
            return ApiResponse<UserProfileResponse>.Error("Invalid credentials");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Pin
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return ApiResponse<UserProfileResponse>.Error("Invalid credentials");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.AddPasswordHash(_passwordHasher.HashPassword(user, request.Pin));
        }

        var response = GenerateJWtAndMapToUserProfileResponse(user);

        // user.SetRefreshToken(response.Jwt.RefreshToken);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<UserProfileResponse>.Success(response);
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

    private UserProfileResponse GenerateJWtAndMapToUserProfileResponse(UserProfile user)
    {
        var token = _jwtTokenProvider.GenerateUserJwtToken(user);

        var jwt = new Jwt(token);
        return new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.PhoneNumber,
            user.ImageUrl,
            jwt
        );
    }
}
