namespace src.Features
{
    public static class FeaturesServiceExtention
    {
        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseCommandHandlerAsync<,>), typeof(BaseComandHandlerAsync<,>));
            return services;
        }
    }
}
