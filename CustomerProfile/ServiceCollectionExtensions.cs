using CustomerProfile.Data;
using CustomerProfile.External;
using CustomerProfile.Global;
using CustomerProfile.Services;
using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CustomerProfile;

internal static class ServiceCollectionExtensions
{
    private static IServiceCollection AddCustomerDbContext(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));
        //{
        //    options.Name =
        //        Environment.GetEnvironmentVariable("DB_NAME")
        //        ?? throw new ServiceException("DB_NAME environment variable is not set.");
        //    options.User =
        //        Environment.GetEnvironmentVariable("DB_USERNAME")
        //        ?? throw new ServiceException("DB_USER environment variable is not set.");
        //    options.Host =
        //        Environment.GetEnvironmentVariable("DB_HOST")
        //        ?? throw new ServiceException("DB_HOST environment variable is not set.");
        //    options.Password =
        //        Environment.GetEnvironmentVariable("DB_PASSWORD")
        //        ?? throw new ServiceException("DB_PASSWORD environment variable is not set.");
        //    options.Port =
        //        Environment.GetEnvironmentVariable("DB_PORT")
        //        ?? throw new ServiceException("DB_PORT environment variable is not set.");
        //});

        services.AddOptions<DatabaseOptions>().ValidateDataAnnotations().ValidateOnStart();

        // configure db context
        services.AddDbContext<UserProfileDbContext>(
            (serviceProvider, options) =>
            {
                var ds = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                string connString =
                    $"Host={ds.Host};Database={ds.Name};Username={ds.User};Password={ds.Password};Port={ds.Port}";

                options.UseNpgsql(connString, npgSqlOptions =>
                {
                    npgSqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null
                    );
                });
            }
        );

        return services;
    }

    private static IServiceCollection AddFeaturesServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<NinBvnService>();

        services.AddSingleton<IFaceDetector>(_ =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
        );
        services.AddSingleton(_ => FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator());
        services.AddSingleton<FaceRecognitionService>();

        return services;
    }

    private static IServiceCollection AddQuickVerifyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuickVerifySettings>(configuration.GetSection("QuickVerifySettings"));

        services.AddOptions<QuickVerifySettings>().ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient<QuickVerifyBvnNinService>(
            (provider, client) =>
            {
                var quick = provider.GetRequiredService<IOptions<QuickVerifySettings>>().Value;
                client.BaseAddress = new Uri(quick.BaseUrl);
                client.DefaultRequestHeaders.Add(quick.AuthPrefix, quick.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        );
        return services;
    }

    public static IServiceCollection AddCustomServiceExtentions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomerDbContext(configuration);
        services.AddFeaturesServices();
        services.AddQuickVerifyServices(configuration);

        return services;
    }
}
