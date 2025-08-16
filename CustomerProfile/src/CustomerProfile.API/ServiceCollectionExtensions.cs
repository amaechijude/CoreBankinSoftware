using CustomerAPI.Data;
using CustomerAPI.Global;
using CustomerAPI.Services;
using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace UserProfile.API
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerDbContext(this IServiceCollection services, string? connString="")
        {
            DotNetEnv.Env.Load();

            services.Configure<DatabaseOptions>(options =>
            {
                options.DatabaseName = Environment.GetEnvironmentVariable("DB_NAME")
                    ?? throw new ServiceException("DB_NAME environment variable is not set.");
                options.DatabaseUser = Environment.GetEnvironmentVariable("DB_USER")
                    ?? throw new ServiceException("DB_USER environment variable is not set.");
                options.DatabaseHost = Environment.GetEnvironmentVariable("DB_HOST")
                    ?? throw new ServiceException("DB_HOST environment variable is not set.");
                options.DatabasePassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                    ?? throw new ServiceException("DB_PASSWORD environment variable is not set.");
                options.DatabasePort = Environment.GetEnvironmentVariable("DB_PORT")
                    ?? throw new ServiceException("DB_PORT environment variable is not set.");
            });

            services.AddOptions<DatabaseOptions>()
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // configure db context
            services.AddDbContext<CustomerDbContext>((serviceProvider, options) =>
            {
                var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                if (string.IsNullOrWhiteSpace(connString))
                {
                    options.UseNpgsql(dbOptions.ConnectionString);
                }
                else
                {
                    options.UseNpgsql(connString);
                }
            });

            return services;
        }

        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped<OnboardingService>();

            services.AddSingleton<IFaceDetector>(_ =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
            );
            services.AddSingleton(_ =>
            FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator()
            );
            services.AddSingleton<FaceRecognitionService>();

            return services;
        }

        public static IServiceCollection AddQuickVerifyServices(this IServiceCollection services)
        {

            services.Configure<QuickVerifySettings>(options =>
            {
                DotNetEnv.Env.TraversePath();
                options.BaseUrl = Environment.GetEnvironmentVariable("QUICKVERIFY_BASE_URL")
                    ?? throw new ServiceException("QUICK_VERIFY_BASE_URL environment variable is not set.");
                options.ApiKey = Environment.GetEnvironmentVariable("QUICKVERIFY_API_KEY")
                    ?? throw new ServiceException("QUICK_VERIFY_API_KEY environment variable is not set.");
                options.AuthPrefix = Environment.GetEnvironmentVariable("QUICKVERIFY_AUTH_PREFIX")
                    ?? throw new ServiceException("QUICK_VERIFY_AUTH_PREFIX environment variable is not set.");
            });

            services.AddOptions<QuickVerifySettings>()
                .ValidateDataAnnotations()
                .ValidateOnStart();


            services.AddHttpClient<QuickVerifyBvnNinService>((provider, client) =>
            {
                var quick = provider.GetRequiredService<IOptions<QuickVerifySettings>>().Value;
                client.BaseAddress = new Uri(quick.BaseUrl);
                client.DefaultRequestHeaders.Add(quick.AuthPrefix, quick.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            return services;
        }

        public static IServiceCollection AddCustomException(this ServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<CustomExceptionHandler>();
            return services;
        }
        internal sealed class ServiceException(string message) : Exception(message);
    }
}