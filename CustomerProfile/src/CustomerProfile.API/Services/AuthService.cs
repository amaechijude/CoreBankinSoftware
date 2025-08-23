using System.Threading.Channels;
using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.Entities;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Messaging.SMS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Services
{
    public class AuthService(
        UserProfileDbContext context,
        ILogger<AuthService> logger,
        JwtTokenProviderService jwtTokenProvider,
        Channel<SendSMSCommand> smsChannel)
    {
        private readonly UserProfileDbContext _context = context;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;

        private readonly PasswordHasher<UserProfile> _passwordHasher = new();


        public async Task<ApiResponse<OnboardingResponse>> InitiateOnboard(OnboardingRequest command)
        {
            var validator = new OnboardingRequestValidator();
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return ApiResponse<OnboardingResponse>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }

            var user = await _context.UserProfiles
                .FirstOrDefaultAsync(c => c.PhoneNumber == command.PhoneNumber);
            if (user is not null)
            {
                var message = "Someone tried to sign up with your phone number";
                await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber, message));
                return ApiResponse<OnboardingResponse>.Error("Phone Number already registered, Try Login");
            }

            var existingCode = await _context.VerificationCodes
                .Where(v => v.UserPhoneNumber == command.PhoneNumber && v.UserEmail == command.Email)
                .FirstOrDefaultAsync();

            if (existingCode is not null)
            {
                existingCode.UpdateCode();
                await _context.SaveChangesAsync();
                var (tokn, expireIn) = _jwtTokenProvider
                    .GenerateVerificationResponseJwtToken(existingCode);
                await EnqueueSms(existingCode.UserPhoneNumber, existingCode.Code);
                return ApiResponse<OnboardingResponse>
                    .Success(new OnboardingResponse(tokn, expireIn));
            }
            var newCode = VerificationCode.CreateNew(command);

            await _context.VerificationCodes.AddAsync(newCode);
            await _context.SaveChangesAsync();
            var (token, expiresIn) = _jwtTokenProvider
                .GenerateVerificationResponseJwtToken(newCode);
            await EnqueueSms(command.PhoneNumber, newCode.Code);
            return ApiResponse<OnboardingResponse>
                .Success(new OnboardingResponse(token, expiresIn));

        }

        public async Task<ApiResponse<string>> VerifyOtpAsync(Guid vId, OtpVerifyRequestBody request)
        {
            var verificationCode = await _context.VerificationCodes
                .Where(v => v.Id == vId && v.Code == request.OtpCode)
                .FirstOrDefaultAsync();

            if (verificationCode is null)
                return ApiResponse<string>.Error("Verification Failed");

            if (verificationCode.IsUsed || verificationCode.IsExpired)
                return ApiResponse<string>.Error("Verification Expired");

            verificationCode.MarkIsUsedAndCanSetProfile();
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Success("Success");
        }

        public async Task<ApiResponse<UserProfileResponse>> HandleSetProfileAsync(Guid validId, SetProfileRequest request)
        {
            var validator = new SetDetailsRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return ApiResponse<UserProfileResponse>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }

            var vCode = await _context.VerificationCodes.FindAsync(validId);
            if (vCode is null || !vCode.CanSetProfile)
                return ApiResponse<UserProfileResponse>.Error("Request Timeout");

            if( await _context.UserProfiles
                .AnyAsync(u => u.PhoneNumber == vCode.UserPhoneNumber))
                return ApiResponse<UserProfileResponse>.Error("Possible duplicate request, Try Login");

            var user = UserProfile
                .CreateNewUser(vCode.UserPhoneNumber, email: vCode.UserEmail, request.Username);

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.UserProfiles.Add(user);

            // delete vCode with executedeleteasync
            await _context.VerificationCodes
                .Where(v => v.Id == vCode.Id)
                .ExecuteDeleteAsync();

            await _context.SaveChangesAsync();

            return ApiResponse<UserProfileResponse>
                .Success(GenerateJWtAndMapToUserProfileResponse(user));
        }

        public async Task<ApiResponse<UserProfileResponse>> HandleLoginAsync(LoginRequest request)
        {
            var user = await _context.UserProfiles
                .Where(u => u.Username == request.UsernameOrPhone
                || u.PhoneNumber == request.UsernameOrPhone)
                .FirstOrDefaultAsync();

            if (user is null)
                return ApiResponse<UserProfileResponse>.Error("Invalid credentials");

            var verificationResult = _passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return ApiResponse<UserProfileResponse>.Error("Invalid credentials");

            return ApiResponse<UserProfileResponse>
                .Success(GenerateJWtAndMapToUserProfileResponse(user));
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
                jwt);
        }
    }
}