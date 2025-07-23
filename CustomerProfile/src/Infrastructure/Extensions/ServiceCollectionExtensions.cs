using Microsoft.EntityFrameworkCore;
using src.Infrastructure.Data;
using src.Infrastructure.Repository;
using src.Shared.Domain.Interfaces;

namespace src.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerInfraWithDb(this IServiceCollection services, string connectionString)
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


            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWorkRepository>();

            return services;
        }
    }
}