using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using src.Domain.Interfaces;
using src.Infrastructure.Extensions;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var services = new ServiceCollection();
            string testConnectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpassword";
            // Act
            services.AddCustomerDatabaseInfra(testConnectionString);
            services.AddCustomerRepository();
            var serviceProvider = services.BuildServiceProvider();
            // Assert
            Assert.NotNull(serviceProvider.GetService<ICustomerRepository>());
            Assert.NotNull(serviceProvider.GetService<IVerificationCodeRepository>());
            Assert.NotNull(serviceProvider.GetService<IUnitOfWork>());
        }

        [Theory]
        [InlineData("08012345678", true)]
        [InlineData("2348012345678", true)]
        [InlineData("+2348012345678", true)]
        [InlineData("0812345678", true)]
        public void TestValidCustomerCommand(string phoneNumber, bool expectedIsValid)
        {
            // Arrange
            var command = new src.Features.CustomerOnboarding.OnboardingRequest { PhoneNumber = phoneNumber };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(command, null, null);
            // Act
            Validator.TryValidateObject(command, context, validationResults, true);
            // Assert
            if (expectedIsValid)
            {
                Assert.Empty(validationResults);
            }
            else
            {
                Assert.NotEmpty(validationResults);
                Assert.Contains(validationResults, v => v.ErrorMessage == "Invalid phone number format");
            }
        }
    }

}
