using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ControlService.Api.IntegrationTests.Common;

/// <summary>The real API against the shared PostgreSQL container. Every test factory in this
/// project derives from this one, since Program now needs a database to start.</summary>
public class ApiFactory(PostgresContainerFixture postgres) : WebApplicationFactory<Program>
{
    // Fictitious secrets: the real ones live in user secrets and never in source (local environment guide).
    public const string AdminInitialPassword = "senha-inicial-de-teste";

    public static readonly string SigningKey = Convert.ToBase64String(new byte[32].Select(_ => (byte)7).ToArray());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:controlservice"] = postgres.ConnectionString,
            ["Admin:Email"] = "admin@example.com",
            ["Admin:InitialPassword"] = AdminInitialPassword,
            ["Auth:SigningKey"] = SigningKey,
            // High enough that no test hits them; the rate-limit tests lower them (RateLimitedApiFactory).
            ["RateLimiting:SignIn:PermitLimit"] = "1000",
            ["RateLimiting:Refresh:PermitLimit"] = "1000",
        }));
    }
}
