using System.Threading.Channels;
using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.Entities;
using CustomerAPI.Messaging.SMS;
using Microsoft.EntityFrameworkCore;
using UserProfile.API.Features;

namespace CustomerAPI.Services
{
    public class OnboardingService(
        CustomerDbContext context,
        ILogger<OnboardingService> logger,
        Channel<SendSMSCommand> smsChannel,
        QuickVerifyBvnNinService bvnNinService,
        FaceRecognitionService faceRecognitionService)
    {
        private readonly CustomerDbContext _context = context;
        private readonly ILogger<OnboardingService> _logger = logger;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
        private readonly QuickVerifyBvnNinService _bvnNinService = bvnNinService;
        private readonly FaceRecognitionService _faceRecognitionService = faceRecognitionService;


        public async Task<ResultResponse<OnboardingResponse>> HandleAsync(OnboardingRequest command)
        {
            var validator = new OnboardingRequestValidator();
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return ResultResponse<OnboardingResponse>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }

            var normalizePhoneNumber = NormalizePhoneNumber(command.PhoneNumber);

            var user = await _context.Customers
                .Where(u => u.PhoneNumber == normalizePhoneNumber
                        || u.PhoneNumber == command.PhoneNumber)
                .FirstOrDefaultAsync();
            if (user is not null)
            {
                var message = "Someone tried to sign up with your phone number";
                await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber, message));
                return ResultResponse<OnboardingResponse>.Error("Phone Number already register");
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
                return ResultResponse<OnboardingResponse>.Success(
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
            return ResultResponse<OnboardingResponse>
                .Success(new
                OnboardingResponse(
                    newCode.Id.ToString(),
                    newCode.ExpiryDuration)
                );

        }

        public async Task<ResultResponse<string>> VerifyRegistrationOtpAsync(string code, string token)
        {
            bool isValidGuid = Guid.TryParse(token, out Guid Id);
            if (!isValidGuid)
                return ResultResponse<string>.Error("Verification failed");

            var vCode = await _context.VerificationCodes
                .Where(vc => vc.Id == Id && vc.Code == code)
                .FirstOrDefaultAsync();

            if (vCode is null)
                return ResultResponse<string>.Error("Verification failed");
            if (vCode.IsUsed || vCode.IsExpired)
                return ResultResponse<string>.Error("Otp is Used or Expired");

            var normalizePhoneNumber = NormalizePhoneNumber(vCode.UserPhoneNumber);

            // create customer
            var user = await _context.Customers
                .Where(u => u.PhoneNumber == normalizePhoneNumber
                        || u.PhoneNumber == vCode.UserPhoneNumber)
                .FirstOrDefaultAsync();

            if (user is null)
            {
                var newCustomer = new Customer(NormalizePhoneNumber(vCode.UserPhoneNumber));
                vCode.MarkAsUsed();
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
            }

            return ResultResponse<string>.Success("Success");
        }

        public async Task<ResultResponse<bool>> NinSearch(NinSearchRequest request)
        {
            var validator = new NinRequestValidator();
            var validate = await validator.ValidateAsync(request);
            if (!validate.IsValid)
            {
                return ResultResponse<bool>
                    .Error(validate.Errors.Select(e => new { e.ErrorMessage, e.AttemptedValue}));
            }

            //NINAPIResponse? response = await _bvnNinService.NINSearchRequest(request); 
            return ResultResponse<bool>.Success(true);
        }

        private async Task EnqueueSms(string phoneNumber, string code)
        {
            var message = $"Your Otp is {code} ";
            var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
            await _smsChannel.Writer.WriteAsync(sendSmsCommand);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim().Replace("-", "").Replace(" ", "");

            if (phoneNumber.StartsWith("+234")) return phoneNumber;

            return "+234" + phoneNumber[1..];
        }

        private async Task<Customer?> UpdateCustomerWithNin(string phone, NINAPIResponse? ninApi)
        {
            if (ninApi is null || string.IsNullOrWhiteSpace(phone))
                return null;

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
            if (customer is null) return null;

            return customer;
        }
    }
}