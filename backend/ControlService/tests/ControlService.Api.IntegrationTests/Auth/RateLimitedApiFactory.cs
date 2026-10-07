using ControlService.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>The real API with rate limits of two requests per window, so the third request is refused
/// without a test having to send dozens.</summary>
public sealed class RateLimitedApiFactory(PostgresContainerFixture postgres) : AuthApiFactory(postgres)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimiting:SignIn:PermitLimit"] = "2",
            ["RateLimiting:Refresh:PermitLimit"] = "2",
        }));
    }
}
