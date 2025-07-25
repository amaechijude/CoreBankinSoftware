using Microsoft.EntityFrameworkCore;
using src.Domain.Interfaces;
using src.Infrastructure.Data;
using src.Infrastructure.Repository;

namespace src.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerDatabaseInfra(this IServiceCollection services, string connectionString)
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
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorCodesToAdd: null
                    );
                });
            });
            return services;
        }

        public static IServiceCollection AddCustomerRepository(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWorkRepository>();
            return services;
        }
    }
    internal sealed class ServiceException(string message) : Exception(message);
}