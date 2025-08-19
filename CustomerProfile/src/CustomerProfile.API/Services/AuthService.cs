using System.Threading.Channels;
using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.Entities;
using CustomerAPI.Global;
using CustomerAPI.Messaging.SMS;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Services
{
    public class AuthService(
        CustomerDbContext context,
        ILogger<AuthService> logger,
        Channel<SendSMSCommand> smsChannel)
    {
        private readonly CustomerDbContext _context = context;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;


        public async Task<ApiResponse<OnboardingResponse>> HandleAsync(OnboardingRequest command)
        {
            var validator = new OnboardingRequestValidator();
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return ApiResponse<OnboardingResponse>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }

            var normalizePhoneNumber = GlobalUtils.NormalizePhoneNumber(command.PhoneNumber);

            var user = await _context.Customers
                .Where(u => u.PhoneNumber == normalizePhoneNumber
                        || u.PhoneNumber == command.PhoneNumber)
                .FirstOrDefaultAsync();
            if (user is not null)
            {
                var message = "Someone tried to sign up with your phone number";
                await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber, message));
                return ApiResponse<OnboardingResponse>.Error("Phone Number already register");
            }


            var existingCode = await _context
                .VerificationCodes
                .FirstOrDefaultAsync(vc =>
                vc.UserPhoneNumber == normalizePhoneNumber
                );

            if (existingCode is not null)
            {
                existingCode.UpdateCode();
                await _context.SaveChangesAsync();
                await EnqueueSms(existingCode.UserPhoneNumber, existingCode.Code);
                return ApiResponse<OnboardingResponse>.Success(
                    new OnboardingResponse(
                        existingCode.Id.ToString(),
                        existingCode.ExpiryDuration
                        )
                    );
            }
            var newCode = new VerificationCode(normalizePhoneNumber);
            await _context.VerificationCodes.AddAsync(newCode);
            await _context.SaveChangesAsync();
            await EnqueueSms(normalizePhoneNumber, newCode.Code);
            return ApiResponse<OnboardingResponse>
                .Success(new
                OnboardingResponse(
                    newCode.Id.ToString(),
                    newCode.ExpiryDuration)
                );

        }

        public async Task<ApiResponse<string>> VerifyRegistrationOtpAsync(string code, string token)
        {
            bool isValidGuid = Guid.TryParse(token, out Guid Id);
            if (!isValidGuid)
                return ApiResponse<string>.Error("Verification failed");

            var vCode = await _context.VerificationCodes
                .Where(vc => vc.Id == Id && vc.Code == code)
                .FirstOrDefaultAsync();

            if (vCode is null)
                return ApiResponse<string>.Error("Verification failed");
            if (vCode.IsUsed || vCode.IsExpired)
                return ApiResponse<string>.Error("Otp is Used or Expired");

            var normalizePhoneNumber = GlobalUtils.NormalizePhoneNumber(vCode.UserPhoneNumber);

            // create customer
            var user = await _context.Customers
                .Where(u => u.PhoneNumber == normalizePhoneNumber
                        || u.PhoneNumber == vCode.UserPhoneNumber)
                .FirstOrDefaultAsync();

            if (user is null)
            {
                var newCustomer = new Customer {PhoneNumber = vCode.UserPhoneNumber};
                vCode.MarkAsUsed();
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
            }

            return ApiResponse<string>.Success("Success");
        }

        private async Task EnqueueSms(string phoneNumber, string code)
        {
            var message = $"Your Otp is {code} ";
            var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
            await _smsChannel.Writer.WriteAsync(sendSmsCommand);
        }
    }
}