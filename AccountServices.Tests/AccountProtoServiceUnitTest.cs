using System.Reflection;
using AccountServices.Data;
using AccountServices.Entities;
using AccountServices.Entities.Enums;
using AccountServices.Services;
using AccountServices.Validators;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SharedGrpcContracts.Protos.Account.Operations.V1;
using Testcontainers.PostgreSql;

namespace AccountServices.Tests;

public sealed class AccountProtoServiceUnitTest : IAsyncLifetime
{
    // 1. Container Definition
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    // 2. Fields (Initialized in InitializeAsync)
    private AccountDbContext _dbContext = null!;
    private AccountProtoService _service = null!;

    // Mocks
    private readonly CreateAccountRequestValidator _validator;
    private readonly CustomResiliencePolicy _resiliencePolicy;
    private readonly ServerCallContext _context;

    public AccountProtoServiceUnitTest()
    {
        // Initialize Mocks in Constructor (safe)
        _validator = Substitute.For<CreateAccountRequestValidator>();
        _resiliencePolicy = new CustomResiliencePolicy();
        _context = Substitute.For<ServerCallContext>();
    }

    public async Task InitializeAsync()
    {
        // A. Start Container
        await _postgres.StartAsync();

        // B. Setup Database Context (Only AFTER container starts)
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new AccountDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        // C. Initialize Service with the connected Context
        _service = new AccountProtoService(_dbContext, _validator, _resiliencePolicy);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            PhoneNumber = "08012345678",
            AccountTypeRequest = AccountTypeRequest.Personal,
            AccountName = "Test Account",
        };

        _validator
            .ValidateAsync(
                Arg.Any<ValidationContext<CreateAccountRequest>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new ValidationResult());

        // Act
        var response = await _service.CreateAccount(request, _context);

        // Assert
        Assert.True(response.Success);
        Assert.True(string.IsNullOrWhiteSpace(response.Error));

        // Use the SAME context instance to verify persistence
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
            a.AccountName == "Test Account"
        );
        Assert.NotNull(account);
        Assert.Equal("08012345678", account.PhoneNumber);
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnError_WhenValidationFails()
    {
        // Arrange
        var request = new CreateAccountRequest();
        var validationResult = new ValidationResult(
            [new ValidationFailure("Prop", "Error message")]
        );

        _validator
            .ValidateAsync(
                Arg.Any<ValidationContext<CreateAccountRequest>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(validationResult);

        // Act
        var response = await _service.CreateAccount(request, _context);

        // Assert
        Assert.False(response.Success);
        Assert.Contains("Error message", response.Error);
    }

    [Fact]
    public async Task IntraBankNameEnquiry_ShouldReturnAccount_WhenFound()
    {
        // Arrange
        var account = Account.Create(
            Guid.NewGuid(),
            "08011111111",
            AccountType.Personal,
            "Test User"
        );
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new IntraBankNameEnquiryRequest { AccountNumber = account.AccountNumber };

        // Act
        var response = await _service.IntraBankNameEnquiry(request, _context);

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Test User", response.AccountName);
    }

    [Fact]
    public async Task IntraBankNameEnquiry_ShouldReturnFailed_WhenAccountNotFound()
    {
        // Arrange
        var request = new IntraBankNameEnquiryRequest { AccountNumber = "1234567890" };

        // Act
        var response = await _service.IntraBankNameEnquiry(request, _context);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Account not found", response.Error);
    }

    [Fact]
    public async Task Deposit_ShouldReturnSuccess_WhenAmountIsValid()
    {
        // Arrange
        var account = Account.Create(
            Guid.NewGuid(),
            "08022222222",
            AccountType.Personal,
            "Test Deposit"
        );
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new DepositRequest { AccountNumber = account.AccountNumber, Amount = 100 };

        // Act
        var response = await _service.Deposit(request, _context);

        // Assert
        Assert.True(response.Success);

        // Reload entity to ensure we get fresh data from DB, not cached data
        await _dbContext.Entry(account).ReloadAsync();
        Assert.Equal(100, account.Balance);
    }

    [Fact]
    public async Task Deposit_ShouldReturnError_WhenAccountNotFound()
    {
        var request = new DepositRequest { AccountNumber = "1234567890", Amount = 100 };
        var response = await _service.Deposit(request, _context);
        Assert.False(response.Success);
        Assert.Equal("Account not found", response.Error);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnSuccess_WhenBalanceIsSufficient()
    {
        // Arrange
        var account = Account.Create(
            Guid.NewGuid(),
            "08033333333",
            AccountType.Personal,
            "Test Withdraw"
        );
        CreditAccount(account, 500); // Helper method
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new WithdrawRequest { AccountNumber = account.AccountNumber, Amount = 100 };

        // Act
        var response = await _service.Withdraw(request, _context);

        // Assert
        Assert.True(response.Success);

        await _dbContext.Entry(account).ReloadAsync();
        Assert.Equal(400, account.Balance);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnError_WhenInsufficientFunds()
    {
        // Arrange
        var account = Account.Create(
            Guid.NewGuid(),
            "08044444444",
            AccountType.Personal,
            "Poor User"
        );
        CreditAccount(account, 50);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new WithdrawRequest { AccountNumber = account.AccountNumber, Amount = 100 };

        // Act
        var response = await _service.Withdraw(request, _context);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Insufficient funds", response.Error);

        await _dbContext.Entry(account).ReloadAsync();
        Assert.Equal(50, account.Balance);
    }

    [Fact]
    public async Task Transfer_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var fromAccount = Account.Create(customerId, "08055555555", AccountType.Personal, "Sender");
        CreditAccount(fromAccount, 1000);

        var toAccount = Account.Create(
            Guid.NewGuid(),
            "08066666666",
            AccountType.Personal,
            "Receiver"
        );

        _dbContext.Accounts.AddRange(fromAccount, toAccount);
        await _dbContext.SaveChangesAsync();

        var request = new TransferRequest
        {
            CustomerId = customerId.ToString(),
            ToAccountNumber = toAccount.AccountNumber,
            Amount = 500,
        };

        // Act
        var response = await _service.Transfer(request, _context);

        // Assert
        Assert.True(response.Success);

        await _dbContext.Entry(fromAccount).ReloadAsync();
        await _dbContext.Entry(toAccount).ReloadAsync();

        Assert.Equal(500, fromAccount.Balance);
        Assert.Equal(500, toAccount.Balance);
    }

    // Helper to access private method in Account entity
    private static void CreditAccount(Account account, decimal amount)
    {
        var method = typeof(Account).GetMethod(
            "CreditAccount",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (method != null)
        {
            method.Invoke(account, [amount]);
        }
        else
        {
            throw new Exception("CreditAccount method not found");
        }
    }
}
























// using System.Reflection;
// using AccountServices.Data;
// using AccountServices.Entities;
// using AccountServices.Entities.Enums;
// using AccountServices.Services;
// using AccountServices.Validators;
// using FluentValidation;
// using FluentValidation.Results;
// using Grpc.Core;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using NSubstitute;
// using SharedGrpcContracts.Protos.Account.Operations.V1;
// using Testcontainers.PostgreSql;
// using Xunit;

// namespace AccountServices.Tests;

// public sealed class AccountProtoServiceUnitTest : IAsyncLifetime
// {
//     // 1. Define the Container
//     private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
//         .WithImage("postgres:17-alpine") // Use lightweight Alpine image
//         .Build();

//     public async Task InitializeAsync()
//     {
//         await _postgres.StartAsync();
//     }

//     public async Task DisposeAsync()
//     {
//         await _postgres.DisposeAsync();
//     }

//     private AccountDbContext DbContext()
//     {
//         var options = new DbContextOptionsBuilder<AccountDbContext>()
//             .UseNpgsql(_postgres.GetConnectionString())
//             .Options;
//         var context = new AccountDbContext(options);
//         context.Database.EnsureCreated();
//         return context;
//     }

//     private AccountDbContext _dbContext => DbContext();

//     private readonly CreateAccountRequestValidator _validator;
//     private readonly CustomResiliencePolicy _resiliencePolicy;
//     private readonly AccountProtoService _service;
//     private readonly ServerCallContext _context;

//     public AccountProtoServiceUnitTest()
//     {
//         _validator = Substitute.For<CreateAccountRequestValidator>();
//         _resiliencePolicy = new CustomResiliencePolicy();
//         _context = Substitute.For<ServerCallContext>();

//         _service = new AccountProtoService(_dbContext, _validator, _resiliencePolicy);
//     }

//     [Fact]
//     public async Task CreateAccount_ShouldReturnSuccess_WhenRequestIsValid()
//     {
//         // Arrange
//         var request = new CreateAccountRequest
//         {
//             CustomerId = Guid.NewGuid().ToString(),
//             PhoneNumber = "08012345678",
//             AccountTypeRequest = AccountTypeRequest.Personal,
//             AccountName = "Test Account",
//         };

//         // Mocking the VIRTUAL ValidateAsync method on AbstractValidator
//         _validator
//             .ValidateAsync(
//                 Arg.Any<ValidationContext<CreateAccountRequest>>(),
//                 Arg.Any<CancellationToken>()
//             )
//             .Returns(new ValidationResult());

//         // Act
//         var response = await _service.CreateAccount(request, _context);

//         // Assert
//         Assert.True(response.Success);
//         Assert.True(string.IsNullOrWhiteSpace(response.Error));

//         var account = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
//             a.AccountName == "Test Account"
//         );
//         Assert.NotNull(account);
//         Assert.Equal("08012345678", account.PhoneNumber);
//     }

//     [Fact]
//     public async Task CreateAccount_ShouldReturnError_WhenValidationFails()
//     {
//         // Arrange
//         var request = new CreateAccountRequest();
//         var validationResult = new ValidationResult(
//             new[] { new ValidationFailure("Prop", "Error message") }
//         );

//         _validator
//             .ValidateAsync(
//                 Arg.Any<ValidationContext<CreateAccountRequest>>(),
//                 Arg.Any<CancellationToken>()
//             )
//             .Returns(validationResult);

//         // Act
//         var response = await _service.CreateAccount(request, _context);

//         // Assert
//         Assert.False(response.Success);
//         Assert.Contains("Error message", response.Error);
//     }

//     [Fact]
//     public async Task IntraBankNameEnquiry_ShouldReturnAccount_WhenFound()
//     {
//         // Arrange
//         var account = Account.Create(
//             Guid.NewGuid(),
//             "08011111111",
//             AccountType.Personal,
//             "Test User"
//         );
//         _dbContext.Accounts.Add(account);
//         await _dbContext.SaveChangesAsync();

//         var request = new IntraBankNameEnquiryRequest { AccountNumber = account.AccountNumber };

//         // Act
//         var response = await _service.IntraBankNameEnquiry(request, _context);

//         // Assert
//         Assert.True(response.Success);
//         Assert.Equal("Test User", response.AccountName);
//     }

//     [Fact]
//     public async Task IntraBankNameEnquiry_ShouldReturnFailed_WhenAccountNotFound()
//     {
//         // Arrange
//         var request = new IntraBankNameEnquiryRequest { AccountNumber = "1234567890" };

//         // Act
//         var response = await _service.IntraBankNameEnquiry(request, _context);

//         // Assert
//         Assert.False(response.Success);
//         Assert.Equal("Account not found", response.Error);
//     }

//     [Fact]
//     public async Task Deposit_ShouldReturnSuccess_WhenAmountIsValid()
//     {
//         // Arrange
//         var account = Account.Create(
//             Guid.NewGuid(),
//             "08022222222",
//             AccountType.Personal,
//             "Test Deposit"
//         );
//         _dbContext.Accounts.Add(account);
//         await _dbContext.SaveChangesAsync();

//         var request = new DepositRequest { AccountNumber = account.AccountNumber, Amount = 100 };

//         // Act
//         var response = await _service.Deposit(request, _context);

//         // Assert
//         Assert.True(response.Success);

//         var updatedAccount = await _dbContext.Accounts.FindAsync(account.Id);
//         Assert.Equal(100, updatedAccount!.Balance);
//     }

//     [Fact]
//     public async Task Deposit_ShouldReturnError_WhenAccountNotFound()
//     {
//         // Arrange
//         var request = new DepositRequest { AccountNumber = "1234567890", Amount = 100 };

//         // Act
//         var response = await _service.Deposit(request, _context);

//         // Assert
//         Assert.False(response.Success);
//         Assert.Equal("Account not found", response.Error);
//     }

//     [Fact]
//     public async Task Withdraw_ShouldReturnSuccess_WhenBalanceIsSufficient()
//     {
//         // Arrange
//         var account = Account.Create(
//             Guid.NewGuid(),
//             "08033333333",
//             AccountType.Personal,
//             "Test Withdraw"
//         );
//         CreditAccount(account, 500);
//         _dbContext.Accounts.Add(account);
//         await _dbContext.SaveChangesAsync();

//         var request = new WithdrawRequest { AccountNumber = account.AccountNumber, Amount = 100 };

//         // Act
//         var response = await _service.Withdraw(request, _context);

//         // Assert
//         Assert.True(response.Success);

//         var updatedAccount = await _dbContext.Accounts.FindAsync(account.Id);
//         Assert.Equal(400, updatedAccount!.Balance);
//     }

//     [Fact]
//     public async Task Withdraw_ShouldReturnError_WhenInsufficientFunds()
//     {
//         // Arrange
//         var account = Account.Create(
//             Guid.NewGuid(),
//             "08044444444",
//             AccountType.Personal,
//             "Poor User"
//         );
//         CreditAccount(account, 50);
//         _dbContext.Accounts.Add(account);
//         await _dbContext.SaveChangesAsync();

//         var request = new WithdrawRequest { AccountNumber = account.AccountNumber, Amount = 100 };

//         // Act
//         var response = await _service.Withdraw(request, _context);

//         // Assert
//         Assert.False(response.Success);
//         Assert.Equal("Insufficient funds", response.Error);
//         Assert.Equal(50, account.Balance); // none was debited
//     }

//     [Fact]
//     public async Task Transfer_ShouldReturnSuccess_WhenValid()
//     {
//         // Arrange
//         var customerId = Guid.NewGuid();
//         var fromAccount = Account.Create(customerId, "08055555555", AccountType.Personal, "Sender");
//         CreditAccount(fromAccount, 1000);

//         var toAccount = Account.Create(
//             Guid.NewGuid(),
//             "08066666666",
//             AccountType.Personal,
//             "Receiver"
//         );

//         _dbContext.Accounts.AddRange(fromAccount, toAccount);
//         await _dbContext.SaveChangesAsync();

//         var request = new TransferRequest
//         {
//             CustomerId = customerId.ToString(),
//             ToAccountNumber = toAccount.AccountNumber,
//             Amount = 500,
//         };

//         // Act
//         var response = await _service.Transfer(request, _context);

//         // Assert
//         Assert.True(response.Success);

//         var updatedFrom = await _dbContext.Accounts.FindAsync(fromAccount.Id);
//         var updatedTo = await _dbContext.Accounts.FindAsync(toAccount.Id);

//         Assert.Equal(500, updatedFrom!.Balance);
//         Assert.Equal(500, updatedTo!.Balance);
//     }

//     private static void CreditAccount(Account account, decimal amount)
//     {
//         var method = typeof(Account).GetMethod(
//             "CreditAccount",
//             BindingFlags.Instance | BindingFlags.NonPublic
//         );
//         if (method != null)
//         {
//             method.Invoke(account, [amount]);
//         }
//         else
//         {
//             // Fallback if method not found (e.g. if it became public or renamed)
//             // But since we just read the file, it is CreditAccount
//             throw new Exception("CreditAccount method not found");
//         }
//     }
// }
