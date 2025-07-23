using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Infrastructure.External.Messaging.SMS;

namespace src.Infrastructure.External.Messaging
{
    public static class MessageServiceExtention
    {
        public static IServiceCollection AddMessageServices(this IServiceCollection services)
        {
            services.Configure<TwilioSettings>(options =>
            {
                options.AccountSid = Environment.GetEnvironmentVariable("TWILLO_ACCOUNT_SID")
                    ?? throw new TwilloException("TWILLO_ACCOUNT_SID is not set.");

                options.AuthToken = Environment.GetEnvironmentVariable("TWILLO_AUTH_TOKEN")
                    ?? throw new TwilloException("TWILLO_AUTH_TOKEN is not set.");

                options.FromPhoneNumber = Environment.GetEnvironmentVariable("TWILLO_FROM_PHONE_NUMBER")
                    ?? throw new TwilloException("TWILLO_FROM_PHONE_NUMBER is not set."); ;
            });
            
            services.AddTransient<TwilioSmsSender>();

            return services;
        }
    }
    internal sealed class TwilloException(string message) : Exception(message);
}