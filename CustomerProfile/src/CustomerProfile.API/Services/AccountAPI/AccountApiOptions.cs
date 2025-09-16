using System.ComponentModel.DataAnnotations;
using CustomerAPI.Global;

namespace CustomerProfile.API.Services.AccountAPI.AccountAPI;

public class AccountApiOptions
{
    [Url, Required, MinLength(6)]
    public string AccountApiUrl { get; set; } = string.Empty;
}

public static class AccountApiOptionsExtensions
{
    public static IServiceCollection AddAccountApiOptions(this IServiceCollection services)
    {
        DotNetEnv.Env.TraversePath();

        services.Configure<AccountApiOptions>(options =>
        {
            options.AccountApiUrl = Environment.GetEnvironmentVariable("ACCOUNT_API_URL")
                ?? throw new ServiceException("ACCOUNT_API_URL environment variable is not set.");
        });

        services.AddOptions<AccountApiOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}