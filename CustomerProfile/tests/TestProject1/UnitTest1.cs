using Microsoft.Extensions.DependencyInjection;
using src.Infrastructure.Extensions;
using src.Shared.Domain.Interfaces;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var services = new ServiceCollection();
            string connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpassword";
            // Act
            services.AddCustomerDatabaseInfra(connectionString);
            services.AddCustomerRepository();
            var serviceProvider = services.BuildServiceProvider();
            // Assert
            Assert.NotNull(serviceProvider.GetService<ICustomerRepository>());
            Assert.NotNull(serviceProvider.GetService<IVerificationCodeRepository>());
            Assert.NotNull(serviceProvider.GetService<IUnitOfWork>());
        }
    }
}
