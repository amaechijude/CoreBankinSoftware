using System.Threading.Channels;
using src.Features.BvnNINVerification;
using src.Infrastructure.External.Messaging.SMS;

namespace src.Infrastructure.External
{
    public static class ExternalServiceExtention
    {
        public static IServiceCollection AddBvnNINVerificationServices(this IServiceCollection services)
        {
            // Load environment variables from .env file
            DotNetEnv.Env.Load();

            // QuickVerify NIN and BVN API settings
            services.Configure<QuickVerifySettings>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("QUICKVERIFY_API_KEY")
                    ?? throw new CustomTwilloException("QUICKVERIFY_API_KEY is not set.");
                options.BaseUrl = Environment.GetEnvironmentVariable("QUICKVERIFY_BASE_URL")
                    ?? throw new CustomTwilloException("QUICKVERIFY_BASE_URL is not set.");
                options.AuthPrefix = Environment.GetEnvironmentVariable("QUICKVERIFY_AUTH_PREFIX")
                    ?? throw new CustomTwilloException("QUICKVERIFY_AUTH_PREFIX is not set.");
            });

            services.AddOptions<QuickVerifySettings>()
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Add HTTP Client for QuickVerify
            // services.AddHttpClient<QuickVerifyBvnNinService>(client =>
            // {
            //     var quickVerifySettings = services
            //         .BuildServiceProvider()
            //         .GetRequiredService<IOptions<QuickVerifySettings>>().Value;

            //     client.BaseAddress = new Uri(quickVerifySettings.BaseUrl);
            //     client.DefaultRequestHeaders.Add(quickVerifySettings.AuthPrefix, quickVerifySettings.ApiKey);
            // })
            //   .ConfigurePrimaryHttpMessageHandler(() =>
            //   {
            //       return new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(15) };
            //   })
            //   .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            return services;
        }

        public static IServiceCollection AddSMSMessageServices(this IServiceCollection services)
        {
            //
            // Load environment variables from .env file
            DotNetEnv.Env.Load();

            // Validate required Twilio environment variables
            services.Configure<TwilioSettings>(options =>
            {
                options.AccountSid = Environment.GetEnvironmentVariable("TWILLO_ACCOUNT_SID")
                    ?? throw new CustomTwilloException("TWILLO_ACCOUNT_SID is not set.");

                options.AuthToken = Environment.GetEnvironmentVariable("TWILLO_AUTH_TOKEN")
                    ?? throw new CustomTwilloException("TWILLO_AUTH_TOKEN is not set.");

                options.FromPhoneNumber = Environment.GetEnvironmentVariable("TWILLO_FROM_PHONE_NUMBER")
                    ?? throw new CustomTwilloException("TWILLO_FROM_PHONE_NUMBER is not set."); ;
            });

            services.AddOptions<TwilioSettings>()
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<TwilioSmsSender>();

            services.AddSingleton(Channel.CreateBounded<SendSMSCommand>(700));

            services.AddHostedService<SMSBackgroundService>();

            return services;
        }
    }
    public sealed class CustomTwilloException(string message) : Exception(message);
}