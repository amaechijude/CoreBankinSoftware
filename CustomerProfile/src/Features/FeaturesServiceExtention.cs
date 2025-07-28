using src.Features.CustomerOnboarding;
//using src.Features.FaceRecognotion;

namespace src.Features
{
    public static class FeaturesServiceExtention
    {
        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            //services.AddScoped(typeof(BaseComandHandlerAsync<,>));
            services.AddScoped<OnboardingCommandHandler>();
            //services.AddScoped<FaceRecognitionService>();
            return services;
        }
    }
}
