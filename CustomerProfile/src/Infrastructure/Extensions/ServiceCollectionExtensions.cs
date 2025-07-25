using Microsoft.EntityFrameworkCore;
using src.Infrastructure.Data;
using src.Infrastructure.Repository;
using src.Shared.Domain.Interfaces;

namespace src.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerDatabaseInfra(this IServiceCollection services)
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

            string? _dbName = Environment.GetEnvironmentVariable("DB_NAME");
            string? _dbUser = Environment.GetEnvironmentVariable("DB_USER");
            string? _dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            string? _dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string? _dbPort = Environment.GetEnvironmentVariable("DB_PORT");

            string connString = $"Server={_dbHost};Database={_dbName};User Id={_dbUser};Password={_dbPassword};Port={_dbPort};";
            
            services.AddDbContext<CustomerDbContext>(options =>
            {
                options.UseNpgsql(connString, npgSqlOptions =>
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