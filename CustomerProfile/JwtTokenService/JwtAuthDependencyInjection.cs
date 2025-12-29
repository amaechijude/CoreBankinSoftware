using CustomerProfile.Global;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CustomerProfile.JwtTokenService;

public sealed record JwtOptions
{
    [Required, MinLength(150)]
    public string SecretKey { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string Issuer { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string Audience { get; set; } = string.Empty;

    public const string SectionName = "JwtOptions";
}

public static class JwtAuthDependencyInjection
{
    private static IServiceCollection AddJwtOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName))
            .AddOptions<JwtOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                beareOptions =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var jwtOptions = serviceProvider
                        .GetRequiredService<IOptions<JwtOptions>>()
                        .Value;
                    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

                    // configure bearer options
                    beareOptions.RequireHttpsMetadata = environment.IsProduction();
                    beareOptions.SaveToken = true;

                    beareOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateIssuer = true,

                        ValidAudience = jwtOptions.Audience,
                        ValidateAudience = true,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
                        ),
                        ValidateIssuerSigningKey = true,

                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                    };

                    beareOptions.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (
                                context.Exception.GetType() == typeof(SecurityTokenExpiredException)
                            )
                            {
                                context.Response.Headers.Append("token-expired", "true");
                            }
                            else
                            {
                                context.Response.Headers.Append("authentication-failed", "true");
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Headers[GlobalUtils.TokenHeaderName];
                            return Task.CompletedTask;
                        },
                    };
                }
            );
        return services;
    }

    public static IServiceCollection AddJwtAuthDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddJwtOptions(configuration)
            .AddScoped<JwtTokenProviderService>()
            .AddJwtAuthentication();
    }
}
