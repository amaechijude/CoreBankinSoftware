using System.Threading.Channels;
using src.Domain.Entities;
using src.Domain.Interfaces;
//using src.Features.FaceRecognotion;
using src.Infrastructure.External.Messaging.SMS;

namespace src.Features.CustomerOnboarding
{
    public class OnboardingCommandHandler(
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<OnboardingCommandHandler> logger,
        //FaceRecognitionService faceRecognitionService,
        Channel<SendSMSCommand> smsChannel) //: BaseComandHandlerAsync<OnboardingRequest, OnboardingResponse>
    {
        private readonly IVerificationCodeRepository _verificationCodeRepository = verificationCodeRepository;
        private readonly ILogger<OnboardingCommandHandler> _logger = logger;
        //private readonly FaceRecognitionService _faceRecognitionService = faceRecognitionService;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;


        public async Task<ResultResponse<OnboardingResponse>> HandleAsync(OnboardingRequest command)
        {
            var validator = new OnboardingRequestValidator();
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for onboarding request: {Errors}", validationResult.Errors);
                return ResultResponse<OnboardingResponse>.Error("Invalid request data.");
            }

            var existingCode = await _verificationCodeRepository.GetAsync(command.PhoneNumber);
            if (existingCode is not null)
            {
                existingCode.UpdateCode();
                await _verificationCodeRepository.SaveChangesAsync();
                await EnqueSms(command.PhoneNumber, existingCode.Code);
                return ResultResponse<OnboardingResponse>.Success(new OnboardingResponse(existingCode));
            }
                var newCode = new VerificationCode(command.PhoneNumber);
                await _verificationCodeRepository.AddAsync(newCode);
                await _verificationCodeRepository.SaveChangesAsync();
                await EnqueSms(command.PhoneNumber, newCode.Code);
                return ResultResponse<OnboardingResponse>.Success(new OnboardingResponse(newCode));
            
        }

        private async Task EnqueSms(string phoneNumber, string code)
        {
            try
            {
                var sendSmsCommand = new SendSMSCommand(phoneNumber, code);
                await _smsChannel.Writer.WriteAsync(sendSmsCommand);
                _logger.LogInformation("SMS command enqueued for phone number: {PhoneNumber}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue SMS command for phone number: {PhoneNumber}", phoneNumber);
            }
        }

        public async Task<ResultResponse<NINResponse>> HandleNinAsync()
        {               
            //var (isValid, embeddings) = await _faceRecognitionService
            //    .MatchUserFaceWithNinImageAndGenerateEmbedding(
            //    ninRequest.Image, ninRequest.Url);
            await Task.Delay(100);

            return ResultResponse<NINResponse>.Success(new NINResponse(true, [2.2f, 4.6f]));
        }

    }
}