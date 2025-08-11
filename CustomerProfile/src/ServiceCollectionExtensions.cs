using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using src.Features.BvnNINVerification;
using src.Features.CustomerOnboarding;
using src.Shared.Data;

namespace src
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerDbContext(this IServiceCollection services, string connectionString)
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

            services.AddDbContext<CustomerDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgSqlOptions =>
                {
                    npgSqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorCodesToAdd: null
                    );
                });
            });
            return services;
        }

        public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped<OnboardingCommandHandler>();

            services.AddHttpClient<QuickVerifyHttpClient>();
            services.AddScoped<QuickVerifyBvnNinService>();

            services.AddSingleton<IFaceDetector>(_ =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
            );
            services.AddSingleton(_ =>
            FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator()
            );
            services.AddSingleton<FaceRecognitionService>();

            return services;
        }
        internal sealed class ServiceException(string message) : Exception(message);
    }
}