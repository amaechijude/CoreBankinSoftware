using System.Threading.Channels;
using AccountOperationsProtosV1;
using Confluent.Kafka;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Testcontainers.PostgreSql;
using TransactionService.Data;
using TransactionService.DTOs.IntraBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.Services;

namespace CoreBankingSoftwareUnitTests.TransactionTests;

public class IntraBankServiceUnitTests(PostgresqlDatabaseFixture databaseFixture)
    : IClassFixture<PostgresqlDatabaseFixture>,
        IAsyncLifetime
{
    // db container
    private readonly PostgreSqlContainer _postgresql = databaseFixture.Postgres;

    // db context & service class
    private TransactionDbContext _dbContext = null!;
    private IntraBankService _service = null!;

    // mock deps with nsubstitue
    private readonly AccountOperationsGrpcService.AccountOperationsGrpcServiceClient _client =
        Substitute.For<AccountOperationsGrpcService.AccountOperationsGrpcServiceClient>();

    private readonly HybridCache _hybridCache = Substitute.For<HybridCache>();

    private readonly IValidator<TransferRequestIntra> _transferIntraValidator = Substitute.For<
        IValidator<TransferRequestIntra>
    >();

    private readonly IValidator<NameEnquiryIntraRequest> _nameEnquiryIntraValidator =
        Substitute.For<IValidator<NameEnquiryIntraRequest>>();

    private readonly ILogger<IntraBankService> _logger = Substitute.For<
        ILogger<IntraBankService>
    >();

    private readonly Channel<OutboxMessage> _channel = Channel.CreateUnbounded<OutboxMessage>();

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseNpgsql(_postgresql.GetConnectionString())
            .Options;

        _dbContext = new TransactionDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
        await _dbContext.SaveChangesAsync();

        // Init service
        _service = new IntraBankService(
            client: _client,
            hybridCache: _hybridCache,
            dbContext: _dbContext,
            transferValidator: _transferIntraValidator,
            nameEnquiryValidator: _nameEnquiryIntraValidator,
            channel: _channel,
            logger: _logger
        );
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private const string _validAccountNumber = "9087654321";

    [Fact]
    public async Task NameEnquiry_ShouldReturnSuccess()
    {
        // Arrange

        var request = new NameEnquiryIntraRequest(_validAccountNumber);
        var expectedResponse = new IntraBankNameEnquiryResponse
        {
            Success = true,
            AccountNumber = _validAccountNumber,
            AccountName = "John Doe",
            BankName = "Guaranty Trust Bank",
        };

        _nameEnquiryIntraValidator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _hybridCache
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<IntraBankNameEnquiryResponse?>>>(),
                Arg.Any<HybridCacheEntryOptions?>(),
                Arg.Any<IEnumerable<string>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(expectedResponse);

        // Act
        var result = await _service.NameEnquiry(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("John Doe", result.Data.AccountName);
        Assert.Equal("Guaranty Trust Bank", result.Data.BankName);
        Assert.Equal(_validAccountNumber, result.Data.AccountNumber);
    }

    [Fact]
    public async Task Transfer_ShouldReturnSuccess()
    {
        // Arrange
        var request = new TransferRequestIntra(
            IsIntraBank: true,
            IdempotencyKey: Guid.NewGuid().ToString(),
            CustomerId: Guid.NewGuid(),
            SessionId: Guid.NewGuid().ToString(),
            SenderAccountNumber: "1234567890",
            SenderAccountName: "Sender",
            DestinationAccountNumber: _validAccountNumber,
            DestinationAccountName: "Receiver",
            Amount: 100,
            Narration: "Test Transfer",
            DeviceInfo: "Device",
            IpAddress: "127.0.0.1",
            Longitude: "0",
            Latitude: "0",
            TransactionChannel: "Mobile"
        );

        _transferIntraValidator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var grpcResponse = new AccountOperationResponse { Success = true };
        var asyncUnaryCall = new AsyncUnaryCall<AccountOperationResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Grpc.Core.Metadata()),
            () => Status.DefaultSuccess,
            () => [],
            () => { }
        );

        _client
            .TransferAsync(Arg.Any<TransferRequest>(), Arg.Any<CallOptions>())
            .Returns(asyncUnaryCall);

        // Act
        var result = await _service.Transfer(request, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Verify DB
        var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t =>
            t.CustomerId == request.CustomerId
            && t.DestinationAccountNumber == request.DestinationAccountNumber
        );
        Assert.NotNull(transaction);
        Assert.Equal(TransactionStatus.Completed, transaction.TransactionStatus);
    }
}
