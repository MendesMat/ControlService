using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ControlService.Api.IntegrationTests.Common;

/// <summary>The real API against the shared PostgreSQL container (T13). Every test factory in this
/// project derives from this one, since Program now needs a database to start.</summary>
public class ApiFactory(PostgresContainerFixture postgres) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:controlservice"] = postgres.ConnectionString,
            ["Admin:Email"] = "admin@example.com",
        }));
    }
}
