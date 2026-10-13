using ControlService.Api.IntegrationTests.Common;
using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>The real API with a clock the test controls (sessions and tokens expire with it) and rate
/// limits high enough that no other test hits them. The refresh cookie is `Secure`, so the
/// clients of this factory talk to `https://localhost`.</summary>
public class AuthApiFactory(PostgresContainerFixture postgres) : ApiFactory(postgres)
{
    public const string ProtectedPath = "/api/v1/test-only/protected";

    // The minimum level each route of the Users screen requires (PERM-03).
    public const string UsersReaderPath = "/api/v1/test-only/users-reader";
    public const string UsersEditorPath = "/api/v1/test-only/users-editor";
    public const string UsersManagerPath = "/api/v1/test-only/users-manager";

    // The exception of PERM-04: listing profiles takes Reader on Permissões or on Usuários.
    public const string ProfilesOrUsersReaderPath = "/api/v1/test-only/profiles-or-users-reader";

    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));

    /// <summary>A client over `https://localhost`, so the `Secure` cookie is honoured. Cookies are handled by
    /// the tests themselves, which need to read `Set-Cookie` and to send several sessions' cookies.</summary>
    public HttpClient CreateHttpsClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = false,
        AllowAutoRedirect = false,
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IStartupFilter, TestOnlyEndpoints>();
        });
    }

    // Stands for "any other endpoint" while the API has no authenticated route outside authentication.
    // It is mapped through MapApiV1() like a real one. A startup filter gets a plain ApplicationBuilder,
    // so it needs its own routing, and authorization must run after that routing selects the endpoint.
    private sealed class TestOnlyEndpoints : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                var api = endpoints.MapApiV1();
                api.MapGet("/test-only/protected", () => "ok");
                api.MapGet("/test-only/users-reader", () => "ok").RequireScreenAccess(ScreenKeys.Users, AccessLevel.Reader);
                api.MapGet("/test-only/users-editor", () => "ok").RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor);
                api.MapGet("/test-only/users-manager", () => "ok").RequireScreenAccess(ScreenKeys.Users, AccessLevel.Manager);
                api.MapGet("/test-only/profiles-or-users-reader", () => "ok")
                    .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Reader, ScreenKeys.Users);
            });
        };
    }
}
