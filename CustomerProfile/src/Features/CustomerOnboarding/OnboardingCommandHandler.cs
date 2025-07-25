using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using src.Domain.Entities;
using src.Domain.Interfaces;
using src.Infrastructure.External.Messaging.SMS;

namespace src.Features.CustomerOnboarding
{
    public class OnboardingCommandHandler(
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<OnboardingCommandHandler> logger,
        Channel<SendSMSCommand> smsChannel) : BaseComandHandlerAsync<OnboardingRequest, string>
    {
        private readonly IVerificationCodeRepository _verificationCodeRepository = verificationCodeRepository;
      
        private readonly ILogger<OnboardingCommandHandler> _logger = logger;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;

        public override async Task<ResultResponse<string>> HandleAsync(OnboardingRequest command)
        {
            var validator = new OnboardingRequestValidator();
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for onboarding request: {Errors}", validationResult.Errors);
                return ResultResponse<string>.Error("Invalid request data.");
            }

            var existingCode = await _verificationCodeRepository.GetAsync(command.PhoneNumber);
            if (existingCode is not null)
            {
                existingCode.UpdateCode();
                await _verificationCodeRepository.SaveChangesAsync();
                await EnqueSms(command.PhoneNumber, existingCode.Code);
                return ResultResponse<string>.Success("Verification Code sent");
            }
                var newCode = new VerificationCode(command.PhoneNumber);
                await _verificationCodeRepository.AddAsync(newCode);
                await _verificationCodeRepository.SaveChangesAsync();
                await EnqueSms(command.PhoneNumber, newCode.Code);
                return ResultResponse<string>.Success("Verification Code sent");
            
        }

        private async Task EnqueSms(string phoneNumber, string code)
        {
            var smsCommand = new SendSMSCommand(phoneNumber, $"Your verification code is: {code}");
            await _smsChannel.Writer.WriteAsync(smsCommand);
        }

    }
}