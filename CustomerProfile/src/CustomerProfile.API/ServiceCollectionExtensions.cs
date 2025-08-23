using CustomerAPI.Data;
using CustomerAPI.External;
using CustomerAPI.Global;
using CustomerAPI.Services;
using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CustomerAPI
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddCustomerDbContext(this IServiceCollection services)
        {
            DotNetEnv.Env.Load();

            services.Configure<DatabaseOptions>(options =>
            {
                options.DatabaseName = Environment.GetEnvironmentVariable("DB_NAME")
                    ?? throw new ServiceException("DB_NAME environment variable is not set.");
                options.DatabaseUsername = Environment.GetEnvironmentVariable("DB_USERNAME")
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
            services.AddDbContext<UserProfileDbContext>((serviceProvider, options) =>
            {
                var ds = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                string connString = $"Host={ds.DatabaseHost};Database={ds.DatabaseName};Username={ds.DatabaseUsername};Password={ds.DatabasePassword};Port={ds.DatabasePort}";

                options.UseNpgsql(connString);
            });

            return services;
        }

        private static IServiceCollection AddFeaturesServices(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<NinBvnService>();

            services.AddSingleton<IFaceDetector>(_ =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
            );
            services.AddSingleton(_ =>
            FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator()
            );
            services.AddSingleton<FaceRecognitionService>();

            return services;
        }

        private static IServiceCollection AddQuickVerifyServices(this IServiceCollection services)
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

        public static IServiceCollection AddCustomServiceExtentions(this IServiceCollection services)
        {
            services = AddCustomerDbContext(services);
            services = AddFeaturesServices(services);
            services = AddQuickVerifyServices(services);

            return services;
        }
    }
}