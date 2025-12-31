using Testcontainers.PostgreSql;

namespace CoreBankingSoftwareUnitTests;

public sealed class PostgresqlDatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } =
        new PostgreSqlBuilder().WithImage("postgres:15-alpine").Build();

    public async Task InitializeAsync() => await Postgres.StartAsync();

    public async Task DisposeAsync() => await Postgres.DisposeAsync();
}
