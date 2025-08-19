using System.ComponentModel.DataAnnotations;
using System.Text;
using CustomerAPI.Global;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerAPI.JwtTokenService
{
    public class JwtOptions
    {
        [Required, MinLength(64)]
        public string SecretKey { get; set; } = string.Empty;
        [Required, MinLength(5)]
        public string Issuer { get; set; } = string.Empty;
        [Required, MinLength(5)]
        public string Audience { get; set; } = string.Empty;
    }

    public static class JwtAuthDependencyInjection
    {
        private static IServiceCollection JwtOptions(this IServiceCollection services)
        {
            services.Configure<JwtOptions>(options =>
            {
                DotNetEnv.Env.TraversePath();
                options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                    ?? throw new ServiceException("JWT_ISSUER environment variable is not set.");
                options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                    ?? throw new ServiceException("JWT_SECRET_KEY environment variable is not set.");
                options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                    ?? throw new ServiceException("JWT_AUDIENCE environment variable is not set.");
            });

            services.AddOptions<JwtOptions>()
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, beareOptions =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var jwtOptions = serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;
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

                        IssuerSigningKey = new
                            SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ValidateIssuerSigningKey = true,

                        RequireSignedTokens = true,
                        RequireExpirationTime = true
                    };

                    beareOptions.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Append("Token-Expired", "true");
                            }
                            else
                            {
                                context.Response.Headers.Append("Authentication-Failed", "true");
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Headers[GlobalUtils.TokenHeaderName];
                            return Task.CompletedTask;
                        }

                    };

                });
            return services;
        }

        public static IServiceCollection AddJwtAuthDependencyInjection(this IServiceCollection services)
        {
            return services
                .JwtOptions()
                .AddScoped<JwtTokenProviderService>()
                .AddJwtAuthentication();
        }
    }
}