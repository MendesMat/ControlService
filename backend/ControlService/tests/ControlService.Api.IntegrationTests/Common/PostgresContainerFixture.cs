using Testcontainers.PostgreSql;
using Xunit;

[assembly: AssemblyFixture(typeof(ControlService.Api.IntegrationTests.Common.PostgresContainerFixture))]

// Every WebApplicationFactory shares one PostgreSQL database: running test collections in
// parallel would let two hosts race to seed the system records at the same time (23505 on
// pk_users). Sequential is also what a real deployment does (one Program starting at a time).
// Configured in xunit.runner.json (parallelizeTestCollections), since
// CollectionBehaviorAttribute.DisableTestParallelization is obsolete in xunit v3.

namespace ControlService.Api.IntegrationTests.Common;

/// <summary>One PostgreSQL container for the whole test assembly: every WebApplicationFactory
/// shares it instead of starting its own, since Program now needs a database to start.</summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.3").Build();

    public string ConnectionString => _container.GetConnectionString();

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
