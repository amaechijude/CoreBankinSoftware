using FaceAiSharp;
using src.Features.BvnNINVerification;
using src.Features.CustomerOnboarding;

namespace src.Features
{
    public static class FeaturesServiceExtention
    {
        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped<OnboardingCommandHandler>();

            services.AddHttpClient<QuickVerifyHttpClient>();
            services.AddScoped<QuickVerifyBvnNinService>();
            services.AddHttpClient();

            services.AddSingleton<IFaceDetector>(_ =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
            );
            services.AddSingleton<IFaceEmbeddingsGenerator>(_ =>
            FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator()
            );
            services.AddScoped<FaceRecognitionService>();

            services.AddHttpClient();
            return services;
        }
    }
}
