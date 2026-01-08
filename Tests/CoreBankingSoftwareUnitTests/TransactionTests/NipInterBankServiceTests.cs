using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.NIBBS;
using TransactionService.Services;

namespace CoreBankingSoftwareUnitTests.TransactionTests;

public class NipInterBankServiceTests(PostgresqlDatabaseFixture databaseFixture)
    : IClassFixture<PostgresqlDatabaseFixture>,
        IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresql = databaseFixture.Postgres;

    // db context & service class
    private TransactionDbContext _dbContext = null!;
    private NipInterBankService _sut = null!;

    // mock deps
    private readonly INibssService _nibssService = Substitute.For<INibssService>();

    private readonly IValidator<FundCreditTransferRequest> _fundCreditTransferValidator =
        Substitute.For<IValidator<FundCreditTransferRequest>>();
    private readonly IValidator<NameEnquiryRequest> _nameEnquiryValidator = Substitute.For<
        IValidator<NameEnquiryRequest>
    >();

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseNpgsql(_postgresql.GetConnectionString())
            .Options;

        _dbContext = new TransactionDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
        await _dbContext.SaveChangesAsync();

        //  init service
        _sut = new NipInterBankService(
            _dbContext,
            _nibssService,
            _nameEnquiryValidator,
            _fundCreditTransferValidator
        );
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetBeneficiaryAccountDetails_ShouldReturnSuccess_WhenNibssCallIsSuccessful()
    {
        // Arrange
        var request = new NameEnquiryRequest(
            SenderAccountNumber: "1234567890",
            SenderBankName: "Test Sender",
            SenderBankNubanCode: "000001",
            DestinationAccountNumber: "0987654321",
            DestinationBankName: "Dest bank name",
            DestinationBankNubanCode: "000002"
        );

        _nameEnquiryValidator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var expectedResponse = new TransactionService.NIBBS.XmlQueryAndResponseBody.NESingleResponse
        {
            ResponseCode = "00",
            AccountNumber = "1234567890",
            AccountName = "Test Account",
            DestinationBankCode = "000002",
            SessionID = "SESSION_NAME",
            ChannelCode = "1",
        };

        _nibssService
            .NameEnquiryAsync(
                Arg.Any<TransactionService.NIBBS.XmlQueryAndResponseBody.NESingleRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((expectedResponse, string.Empty));

        // Act
        var result = await _sut.GetBeneficiaryAccountDetails(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("1234567890", result.Data.AccountNumber);
        Assert.Equal("Test Account", result.Data.AccountName);
        Assert.Equal("000002", result.Data.BankCode);
    }

    [Fact]
    public async Task FundCreditTransfer_ShouldReturnSuccess_WhenNibssCallIsSuccessful()
    {
        // Arrange
        var request = new FundCreditTransferRequest(
            IsIntraBank: false,
            IdempotencyKey: Guid.NewGuid().ToString(),
            CustomerId: Guid.NewGuid(),
            SenderAccountNumber: "0123456789",
            SenderBankName: "Our Bank",
            SenderBankNubanCode: "000001",
            SenderAccountName: "Me",
            DestinationAccountNumber: "9876543210",
            DestinationBankName: "They",
            DestinationBankNubanCode: "000002",
            DestinationAccountName: "Them",
            Amount: 1000m,
            Narration: "Test",
            DeviceInfo: "Device",
            IpAddress: "127.0.0.1",
            Longitude: null,
            Latitude: null,
            TransactionChannel: "Web"
        );

        _fundCreditTransferValidator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var expectedResponse =
            new TransactionService.NIBBS.XmlQueryAndResponseBody.FTSingleCreditResponse
            {
                ResponseCode = "00",
                SessionID = "SESSION123",
                PaymentReference = "REF123",
                AccountName = "Them",
                Amount = 1000m,
                OriginatorName = "Me",
                Narration = "Test",
                DestinationBankCode = "000002",
                ChannelCode = "1",
                AccountNumber = "9876543210",
            };

        _nibssService
            .FundTransferCreditAsync(
                Arg.Any<TransactionService.NIBBS.XmlQueryAndResponseBody.FTSingleCreditRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((expectedResponse, string.Empty));

        // Act
        var result = await _sut.FundCreditTransfer(
            request.CustomerId,
            request,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("REF123", result.Data.TransactionReference);
        Assert.Equal("SESSION123", result.Data.SessionID);
    }
}
