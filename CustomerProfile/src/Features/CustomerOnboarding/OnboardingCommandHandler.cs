using System.Threading.Channels;
using FaceAiSharp;
using SixLabors.ImageSharp.PixelFormats;
using src.Domain.Entities;
using src.Domain.Interfaces;
using src.Features.FaceRecognotion;
using src.Infrastructure.External.Messaging.SMS;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp;

namespace src.Features.CustomerOnboarding
{
    public class OnboardingCommandHandler(
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<OnboardingCommandHandler> logger,
        FaceRecognitionService faceRecognitionService,
        IHttpClientFactory httpClientFactory,
        Channel<SendSMSCommand> smsChannel)
    {
        private readonly IVerificationCodeRepository _verificationCodeRepository = verificationCodeRepository;
        private readonly ILogger<OnboardingCommandHandler> _logger = logger;
        private readonly FaceRecognitionService _faceRecognitionService = faceRecognitionService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
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
            var sendSmsCommand = new SendSMSCommand(phoneNumber, code);
            await _smsChannel.Writer.WriteAsync(sendSmsCommand);
            _logger.LogInformation("SMS command enqueued for phone number: {PhoneNumber}", phoneNumber);
        }

        public async Task<ResultResponse<NINResponse>> HandleNinAsync(NinRequest ninRequest)
        {

            var (isValid, embeddings) = await _faceRecognitionService
               .MatchUserFaceWithNinImageAndGenerateEmbedding(
               ninRequest.Image, ninRequest.Url, _httpClientFactory);

            return ResultResponse<NINResponse>.Success(new NINResponse(isValid, embeddings));

        }

        public async Task<ResultResponse<NINResponse>> TestAiSharp(NinRequest? ninRequest = default)
        {

            using var hc = new HttpClient();
            var groupPhoto = await hc.GetByteArrayAsync(
                "https://raw.githubusercontent.com/georg-jung/FaceAiSharp/master/examples/obama_family.jpg");
            var img = Image.Load<Rgb24>(groupPhoto);

            var det = FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks();
            var rec = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();

            var faces = det.DetectFaces(img);

            var first = faces.First();
            var second = faces.Skip(1).First();

            // AlignFaceUsingLandmarks is an in-place operation so we need to create a clone of img first
            var secondImg = img.Clone();
            rec.AlignFaceUsingLandmarks(img, first.Landmarks!);
            rec.AlignFaceUsingLandmarks(secondImg, second.Landmarks!);

            img.Save("aligned.jpg");

            var embedding1 = rec.GenerateEmbedding(img);
            var embedding2 = rec.GenerateEmbedding(secondImg);

            var dot = embedding1.Dot(embedding2);

            Console.WriteLine($"Dot product: {dot}");
            if (dot >= 0.42)
                return ResultResponse<NINResponse>.Success(new NINResponse(true, embedding1));

            
            return ResultResponse<NINResponse>.Success(new NINResponse(false, embedding2));

        }

    }
}