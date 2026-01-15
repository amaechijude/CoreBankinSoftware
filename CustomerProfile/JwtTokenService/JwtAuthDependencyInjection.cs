using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerProfile.JwtTokenService;

public sealed class JwtOptions
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
    public const string TOKEN_HEADER_NAME = "X-Auth-Token";

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
                        // falback to cookies auth
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.ContainsKey(TOKEN_HEADER_NAME))
                            {
                                context.Token = context.Request.Cookies[TOKEN_HEADER_NAME];
                            }
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
        services
            .Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName))
            .AddOptions<JwtOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JwtTokenProviderService>();
        services.AddJwtAuthentication();

        return services;
    }
}
