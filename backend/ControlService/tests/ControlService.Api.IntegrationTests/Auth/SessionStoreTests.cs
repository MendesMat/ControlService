using System.Security.Cryptography;
using System.Text;
using ControlService.Application.Auth;
using ControlService.Domain.Common;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class SessionStoreTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    [Fact]
    public async Task Session_store_keeps_only_the_hash_of_the_refresh_token() // AUTH-19
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var sessions = scope.ServiceProvider.GetRequiredService<ISessionStore>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var started = await sessions.StartAsync(SystemIds.AdminUser, TestContext.Current.CancellationToken);

        var plainToken = Encoding.UTF8.GetBytes(started.RefreshToken);
        var stored = await db.Sessions.AsNoTracking().SingleAsync(session => session.Id == started.SessionId, TestContext.Current.CancellationToken);
        stored.TokenHash.ShouldBe(SHA256.HashData(plainToken));
        stored.UserId.ShouldBe(SystemIds.AdminUser);
        stored.ExpiresAt.ShouldBe(factory.Clock.GetUtcNow().AddHours(8));
        started.ExpiresAt.ShouldBe(stored.ExpiresAt);
        (await db.Sessions.CountAsync(session => session.TokenHash == plainToken, TestContext.Current.CancellationToken)).ShouldBe(0);
    }
}
