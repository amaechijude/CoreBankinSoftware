using Microsoft.EntityFrameworkCore;
using src.Shared.Domain.Interfaces;
using src.Shared.Infrastructure.Data;
using src.Shared.Infrastructure.Repository;

namespace src.Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString)
        {
            // Register the ApplicationDbContext with the provided connection string
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
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWorkRepository>();

            return services;
        }
    }
}