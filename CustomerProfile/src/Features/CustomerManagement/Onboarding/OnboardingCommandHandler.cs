using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using src.Domain.Entities;
using src.Features.CustomerManagement.BvnNinVerification;
using src.Shared.Data;
using src.Shared.Global;
using src.Shared.Messaging.SMS;

namespace src.Features.CustomerManagement.Onboarding
{
    public class OnboardingCommandHandler(
        CustomerDbContext context,
        ILogger<OnboardingCommandHandler> logger,
        Channel<SendSMSCommand> smsChannel,
        FaceRecognitionService faceRecognitionService)
    {
        private readonly CustomerDbContext _context = context;
        private readonly ILogger<OnboardingCommandHandler> _logger = logger;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
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

            var normalisedPhoneNumber = NormalizePhoneNumber(command.PhoneNumber);

            var user = await _context.Customers
                .Where(u => u.PhoneNumber == normalisedPhoneNumber
                        || u.PhoneNumber == command.PhoneNumber)
                .FirstOrDefaultAsync();
            if (user is not null)
            {
                var message = "Someone tried to sign up with your phone number";
                await _smsChannel.Writer.WriteAsync(new SendSMSCommand(user.PhoneNumber,message));
                return ResultResponse<OnboardingResponse>.Error("Phone Number already register");
            }


            var existingCode = await _context
                .VerificationCodes
                .FirstOrDefaultAsync(vc => 
                vc.UserPhoneNumber == normalisedPhoneNumber
                );

            if (existingCode is not null)
            {
                existingCode.UpdateCode();
                await _context.SaveChangesAsync();
                await EnqueSms(existingCode.UserPhoneNumber, existingCode.Code);
                return ResultResponse<OnboardingResponse>
                    .Success(
                    new OnboardingResponse(
                        existingCode.Id.ToString(),
                        existingCode.ExpiryDuration)
                    );
            }
            var newCode = new VerificationCode(normalisedPhoneNumber);
            await _context.VerificationCodes.AddAsync(newCode);
            await _context.SaveChangesAsync();
            await EnqueSms(normalisedPhoneNumber, newCode.Code);
            return ResultResponse<OnboardingResponse>
                .Success(new 
                OnboardingResponse(
                    newCode.Id.ToString(),
                    newCode.ExpiryDuration)
                );

        }

        public async Task<ResultResponse<string>> VerifyOtpAsync(string code, string token)
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

            // create customer
            var newCustomer = new Customer(NormalizePhoneNumber(vCode.UserPhoneNumber));
        }

        private async Task EnqueSms(string phoneNumber, string code)
        {
            var message = $"Your Otp is {code} ";
            var sendSmsCommand = new SendSMSCommand(phoneNumber, message);
            await _smsChannel.Writer.WriteAsync(sendSmsCommand);
        }

        public async Task<ResultResponse<FaceComparisonResponse>> Compare(SendIformFile send)
        {
            if (send.Image is null)
                return ResultResponse<FaceComparisonResponse>.Error("Null images");

            return await _faceRecognitionService.CompareFaces(send.Image, GlobalConstansts.base64Image);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim().Replace("-", "").Replace(" ", "");

            if (phoneNumber.StartsWith("+234")) return phoneNumber;

            return "+234" + phoneNumber[1..];
        }
        //private static async Task<string> ConvertToBase64Async(IFormFile file)
        //{
        //    using var memoryStream = new MemoryStream();
        //    await file.CopyToAsync(memoryStream);
        //    byte[] fileBytes = memoryStream.ToArray();
        //    return Convert.ToBase64String(fileBytes);
        //}
    }
}