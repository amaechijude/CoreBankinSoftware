using System.Security.Claims;
using System.Threading.Channels;
using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.DTO.BvnNinVerification;
using CustomerProfile.Entities;
using CustomerProfile.External;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Messaging.SMS;
using FluentValidation;
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
    IValidator<NinSearchRequest> ninSearchValidator,
    IPasswordHasher<UserProfile> passwordHasher,
    QuickVerifyBvnNinService quickVerifyBvnNinService,
    CryptographyService cryptographyService
)
{
    private readonly UserProfileDbContext _context = context;
    private readonly ILogger<OnboardService> _logger = logger;
    private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
    private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
    private readonly IValidator<OnboardingRequest> _onboardingValidator = onboardingValidator;
    private readonly IValidator<SetSixDigitPinRequest> _setSixDigitPinValidator =
        setSixDigitPinValidator;
    private readonly IValidator<NinSearchRequest> _ninSearchValidator = ninSearchValidator;

    private readonly IPasswordHasher<UserProfile> _passwordHasher = passwordHasher;
    private readonly QuickVerifyBvnNinService _quickVerifyBvnNinService = quickVerifyBvnNinService;
    CryptographyService _cryptographyService = cryptographyService;

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

    public async Task<ApiResponse<SearchResponse>> SearchNinAsync(
        Guid validUserId,
        NinSearchRequest request,
        CancellationToken ct
    )
    {
        var validator = new NinSearchRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<SearchResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }
        var user = await _context.UserProfiles.FindAsync([validUserId], cancellationToken: ct);
        if (user is null)
        {
            return ApiResponse<SearchResponse>.Error("User not found, try login again");
        }

        var ninSearchResult = await _quickVerifyBvnNinService.NINSearchRequest(request.Nin);
        if (ninSearchResult is null)
        {
            return ApiResponse<SearchResponse>.Error(
                "Nin service is currently unavailable, try again later"
            );
        }

        if (ninSearchResult.Status == false)
        {
            return ApiResponse<SearchResponse>.Error("Nin not found");
        }
        var hashedNin = _cryptographyService.HashSensitiveData(request.Nin);
        user.SetNin(hashedNin);
        await _context.SaveChangesAsync(ct);
        return ApiResponse<SearchResponse>.Success(new SearchResponse("Success", true));
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
