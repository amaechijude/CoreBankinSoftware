using System.Threading.Channels;
using CustomerAPI.Messaging.SMS;

namespace CustomerAPI.Messaging
{
    public static class MessageServiceExtention
    {
        public static IServiceCollection AddSMSMessageServices(this IServiceCollection services)
        {
            DotNetEnv.Env.Load(); // Load environment variables from .env file

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

            services.AddSingleton(Channel.CreateBounded<SendSMSCommand>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = true
                }));

            services.AddHostedService<SMSBackgroundService>();

            return services;
        }
    }
    internal sealed class CustomTwilloException(string message) : Exception(message);
}