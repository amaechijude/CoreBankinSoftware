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
        }
    }
}