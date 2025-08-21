using System.Threading.Channels;
using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.Entities;
using CustomerAPI.Global;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Messaging.SMS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Services
{
    public class AuthService(
        UserDbContext context,
        ILogger<AuthService> logger,
        JwtTokenProviderService jwtTokenProvider,
        Channel<SendSMSCommand> smsChannel)
    {
        private readonly UserDbContext _context = context;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly JwtTokenProviderService _jwtTokenProvider = jwtTokenProvider;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;

        private readonly PasswordHasher<User> _passwordHasher = new();


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

            var user = await _context.Users
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

            //// create new user with the phone number
            //var newUser = User.CreateNewUser(verificationCode.UserPhoneNumber);
            //newUser.LoginPinHash = _passwordHasher.HashPassword(newUser, request.Password);

            //_context.Users.Add(newUser);
            //await _context.SaveChangesAsync();

            return ApiResponse<string>.Success("Success");
        }

        public async Task<ApiResponse<string>> SetDetailsAsync(Guid validId, SetDetailsRequest request)
        {
            var validator = new SetDetailsRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return ApiResponse<string>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }

            var user = await _context.VerificationCodes
                .FirstOrDefaultAsync(c => c.Id == validId);
            throw new NotImplementedException();
        }

        private async Task EnqueueSms(string phoneNumber, string code)
        {
            var message = $"Your Otp is {code} ";
            var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
            await _smsChannel.Writer.WriteAsync(sendSmsCommand);
        }
    }
}