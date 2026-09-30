using ControlService.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>The real API with a clock the test controls (sessions and tokens expire with it) and rate
/// limits high enough that no other test hits them (T12). The refresh cookie is `Secure`, so the
/// clients of this factory talk to `https://localhost`.</summary>
public class AuthApiFactory(PostgresContainerFixture postgres) : ApiFactory(postgres)
{
    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
