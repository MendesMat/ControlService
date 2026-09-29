using ControlService.Api.IntegrationTests.Common;
using ControlService.Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlService.Api.IntegrationTests.Persistence;

/// <summary>Replaces ICurrentUser and TimeProvider with fakes the test controls (T13), so
/// authorship can be asserted without a real sign-in or the real clock.</summary>
public sealed class PersistenceApiFactory(PostgresContainerFixture postgres) : ApiFactory(postgres)
{
    public FakeCurrentUser CurrentUser { get; } = new();

    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICurrentUser>();
            services.AddSingleton<ICurrentUser>(CurrentUser);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
