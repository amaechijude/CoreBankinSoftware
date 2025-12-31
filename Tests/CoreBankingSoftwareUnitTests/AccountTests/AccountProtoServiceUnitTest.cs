using AccountServices;
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

namespace CoreBankingSoftwareUnitTests.AccountTests;

public sealed class AccountProtoServiceUnitTest(PostgresqlDatabaseFixture fixture)
    : IClassFixture<PostgresqlDatabaseFixture>,
        IAsyncLifetime
{
    // 1. Container Reference
    private readonly PostgreSqlContainer _postgres = fixture.Postgres;

    // 2. Fields (Initialized in InitializeAsync)
    private AccountDbContext _dbContext = null!;
    private AccountProtoService _service = null!;

    // Mocks
    private readonly CreateAccountRequestValidator _validator =
        Substitute.For<CreateAccountRequestValidator>();
    private readonly CustomResiliencePolicy _resiliencePolicy = new();
    private readonly ServerCallContext _context = Substitute.For<ServerCallContext>();

    public async Task InitializeAsync()
    {
        // Setup Database Context (Only AFTER container starts)
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new AccountDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        // Clear data to ensure test isolation with shared container
        _dbContext.Accounts.RemoveRange(_dbContext.Accounts);
        await _dbContext.SaveChangesAsync();

        // C. Initialize Service with the connected Context
        _service = new AccountProtoService(_dbContext, _validator, _resiliencePolicy);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
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
    public async Task CreateAccount_ShouldReturnError_OnDuplicate()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            PhoneNumber = "08012345678",
            AccountTypeRequest = AccountTypeRequest.Personal,
            AccountName = "Test Account",
        };

        var request2 = new CreateAccountRequest
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
        var response2 = await _service.CreateAccount(request2, _context);

        // Assert
        Assert.False(response2.Success);

        // make sure second request is not saved
        var r = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
            a.CustomerId == Guid.Parse(request2.CustomerId)
        );
        Assert.Null(r);
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
        account.CreditAccount(500);
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
        account.CreditAccount(50);
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
        fromAccount.CreditAccount(1000);

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
}
