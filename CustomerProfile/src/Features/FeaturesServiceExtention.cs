using src.Features.CustomerOnboarding;

namespace src.Features
{
    public static class FeaturesServiceExtention
    {
        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseCommandHandlerAsync<,>), typeof(BaseComandHandlerAsync<,>));
            services.AddScoped<OnboardingCommandHandler>();
            return services;
        }
    }
}
