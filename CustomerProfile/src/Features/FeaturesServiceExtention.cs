using src.Features.BvnNINVerification;
using src.Features.CustomerOnboarding;
using src.Features.FaceRecognotion;

namespace src.Features
{
    public static class FeaturesServiceExtention
    {
        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped<OnboardingCommandHandler>();

            services.AddHttpClient<QuickVerifyHttpClient>();
            services.AddScoped<QuickVerifyBvnNinService>();
            services.AddScoped<FaceRecognitionService>();
            services.AddHttpClient();
            return services;
        }
    }
}
