using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;
using Testcontainers.PostgreSql;
using TransactionService.Data;
using TransactionService.Services;

namespace CoreBankingSoftwareUnitTests.TransactionTests;

public class UserPreferenceServiceUnitTests(PostgresqlDatabaseFixture fixture)
    : IClassFixture<PostgresqlDatabaseFixture>,
        IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresql = fixture.Postgres;

    private IDbContextFactory<TransactionDbContext> _dbContextFactory = null!;

    private TransactionDbContext _dbContext = null!;
    private UserPreferenceService _sut = null!;

    // mock deps
    private readonly HybridCache _hybridCache = Substitute.For<HybridCache>();
    private CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient _grpcClient =
        Substitute.For<CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient>();

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseNpgsql(_postgresql.GetConnectionString())
            .Options;

        _dbContextFactory = new TestDbContextFactory(options);
        var _dbContext = _dbContextFactory.CreateDbContext();
        await _dbContext.Database.EnsureCreatedAsync();

        _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}

// Helper class for creating DbContext instances in tests
public sealed class TestDbContextFactory(DbContextOptions<TransactionDbContext> options)
    : IDbContextFactory<TransactionDbContext>
{
    public TransactionDbContext CreateDbContext()
    {
        return new TransactionDbContext(options);
    }

    public Task<TransactionDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(CreateDbContext());
    }
}
