using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using ControlService.Application.Auth;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ControlService.Infrastructure.Auth;

/// <summary>Refresh tokens are 32 random bytes; only their SHA-256 is stored (AUTH-19). A session is
/// valid while now &lt; expires_at, and every use slides that time forward (ADR-0032, T4).</summary>
public sealed class SessionStore(AppDbContext db, IOptions<AuthOptions> options, TimeProvider timeProvider) : ISessionStore
{
    private TimeSpan IdleTime => TimeSpan.FromHours(options.Value.RefreshTokenIdleHours);

    public async Task<SessionTokens> StartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var token = NewToken();
        var session = new UserSession
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = Hash(token),
            CreatedAt = now,
            ExpiresAt = now + IdleTime,
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return new SessionTokens(session.Id, token, session.ExpiresAt);
    }

    public async Task<RotatedSession?> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var oldHash = Hash(refreshToken);
        var newToken = NewToken();
        var newHash = Hash(newToken);
        var expiresAt = now + IdleTime;

        // One UPDATE: two refreshes with the same token cannot both match the old hash (T4).
        var rotated = await db.Sessions
            .Where(session => session.TokenHash == oldHash && session.ExpiresAt > now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(session => session.TokenHash, newHash)
                    .SetProperty(session => session.ExpiresAt, expiresAt),
                cancellationToken);
        if (rotated == 0)
        {
            return null;
        }

        var session = await db.Sessions.AsNoTracking()
            .SingleAsync(candidate => candidate.TokenHash == newHash, cancellationToken);
        return new RotatedSession(session.UserId, new SessionTokens(session.Id, newToken, expiresAt));
    }

    public Task EndAsync(Guid sessionId, CancellationToken cancellationToken) =>
        db.Sessions.Where(session => session.Id == sessionId).ExecuteDeleteAsync(cancellationToken);

    public Task EndAllAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Sessions.Where(session => session.UserId == userId).ExecuteDeleteAsync(cancellationToken);

    private static string NewToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    private static byte[] Hash(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
