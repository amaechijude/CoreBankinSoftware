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
    IValidator<OnboardingRequest> onboardingValidator
)
{
    private readonly UserProfileDbContext _context = context;
    private readonly ILogger<OnboardService> _logger = logger;
    private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
    private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
    private readonly IValidator<OnboardingRequest> _onboardingValidator = onboardingValidator;

    private readonly PasswordHasher<UserProfile> _passwordHasher = new();

    public async Task<ApiResponse<OnboardingResponse>> InitiateOnboard(OnboardingRequest command, CancellationToken ct)
    {
        var validationResult = await _onboardingValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<OnboardingResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }

        var user = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(c =>
            c.PhoneNumber == command.PhoneNumber
, cancellationToken: ct);
        if (user is not null)
        {
            var message = "Someone tried to sign up with your phone number";
            await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber, message), ct);
            return ApiResponse<OnboardingResponse>.Error(
                "Phone Number already registered, Try Login"
            );
        }

        return await HandleOtp(command.PhoneNumber, command.Email);
    }

    public async Task<ApiResponse<string>> VerifyOtpAsync(Guid vId, OtpVerifyRequestBody request, CancellationToken ct)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
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

        verificationCode.MarkIsUsedAndCanSetProfile();
        var user = CreateUser(verificationCode);
        _context.UserProfiles.Add(user);

        //var message = 
        await _context.SaveChangesAsync(ct);
        await _context.VerificationCodes.Where(v => v.Id == vId).ExecuteDeleteAsync(ct);
        await transaction.CommitAsync(ct);

        return ApiResponse<string>.Success("Success");
    }

    public async Task<ApiResponse<UserProfileResponse>> HandleLoginAsync(LoginRequest request)
    {
        var user = await _context
            .UserProfiles.Where(u =>
                u.Username == request.UsernameOrPhone || u.PhoneNumber == request.UsernameOrPhone
            )
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return ApiResponse<UserProfileResponse>.Error("Invalid credentials");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return ApiResponse<UserProfileResponse>.Error("Invalid credentials");
        }

        return ApiResponse<UserProfileResponse>.Success(
            GenerateJWtAndMapToUserProfileResponse(user)
        );
    }

    public async Task<ApiResponse<OnboardingResponse>> HandleForgotPasswordAsync(
        ForgotPasswordRequest request
    )
    {
        var validator = new ForgotPasswordRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<OnboardingResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }
        var user = await _context.UserProfiles.FirstOrDefaultAsync(u =>
            u.PhoneNumber == request.PhoneNumber
        );

        if (user is null)
        {
            return ApiResponse<OnboardingResponse>.Error("User not found");
        }

        return await HandleOtp(user.PhoneNumber, "");
    }

    public async Task<ApiResponse<string>> HandleResetPasswordAsync(
        Guid validId,
        ResetPasswordRequest request
    )
    {
        var verificationCode = await _context
            .VerificationCodes.Where(v => v.Id == validId && v.Code == request.OtpCode)
            .FirstOrDefaultAsync();

        if (verificationCode is null)
        {
            return ApiResponse<string>.Error("Reset Password Failed");
        }

        if (verificationCode.IsUsed || verificationCode.IsExpired)
        {
            return ApiResponse<string>.Error("Otp Expired");
        }

        var user = await _context.UserProfiles.FirstOrDefaultAsync(u =>
            u.PhoneNumber == verificationCode.UserPhoneNumber
        );
        if (user is null)
        {
            return ApiResponse<string>.Error("Password reset failed");
        }

        var passwordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.AddPasswordHash(passwordHash);

        await _context.SaveChangesAsync();
        await _context.VerificationCodes.Where(v => v.Code == request.OtpCode).ExecuteDeleteAsync();

        return ApiResponse<string>.Success("Password reset successful, login");
    }

    private async Task<ApiResponse<OnboardingResponse>> HandleOtp(string phoneNumber, string? email)
    {
        var existingCode = await _context
            .VerificationCodes.Where(v => v.UserPhoneNumber == phoneNumber)
            .FirstOrDefaultAsync();

        if (existingCode is null)
        {
            var newCode = VerificationCode.CreateNew(phoneNumber, email);

            await _context.VerificationCodes.AddAsync(newCode);
            await _context.SaveChangesAsync();
            var (token, expiresIn) = _jwtTokenProvider.GenerateVerificationResponseJwtToken(newCode);
            await EnqueueSms(phoneNumber, newCode.Code);
            return ApiResponse<OnboardingResponse>.Success(new OnboardingResponse(token, expiresIn));
        }
        existingCode.UpdateCode();
        await _context.SaveChangesAsync();
        var (tokn, expireIn) = _jwtTokenProvider.GenerateVerificationResponseJwtToken(
            existingCode
        );
        await EnqueueSms(existingCode.UserPhoneNumber, existingCode.Code);
        return ApiResponse<OnboardingResponse>.Success(new OnboardingResponse(tokn, expireIn));
    }

    private async Task EnqueueSms(string phoneNumber, string code)
    {
        var message = $"Your Otp is {code} ";
        var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
        await _smsChannel.Writer.WriteAsync(sendSmsCommand);
    }

    private UserProfileResponse GenerateJWtAndMapToUserProfileResponse(UserProfile user)
    {
        var (token, expiresIn) = _jwtTokenProvider.GenerateUserJwtToken(user);

        var jwt = new Jwt(token, expiresIn);
        return new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.PhoneNumber,
            user.ImageUrl,
            jwt
        );
    }

    private static UserProfile CreateUser(VerificationCode code)
    {
        var username = code.UserEmail[0..code.UserEmail.IndexOf('@')];

        return UserProfile.CreateNewUser(code.UserPhoneNumber, code.UserEmail, username);
    }
}
