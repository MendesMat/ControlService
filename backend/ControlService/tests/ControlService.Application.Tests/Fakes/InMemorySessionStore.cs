using ControlService.Application.Auth;

namespace ControlService.Application.Tests.Fakes;

internal sealed class InMemorySessionStore : ISessionStore
{
    private readonly List<StoredSession> _sessions = [];

    // Counts every token ever issued: numbering by the current count would repeat a token after a rotation.
    private int _issuedTokens;

    public IReadOnlyList<StoredSession> SessionsOf(Guid userId) => _sessions.Where(session => session.UserId == userId).ToArray();

    /// <summary>Stands for the idle time running out: the real store compares dates in SQL, and that
    /// is tested against PostgreSQL.</summary>
    public void Expire(string refreshToken) => _sessions.RemoveAll(session => session.RefreshToken == refreshToken);

    public Task<SessionTokens> StartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var session = new StoredSession(Guid.CreateVersion7(), userId, NewToken());
        _sessions.Add(session);
        return Task.FromResult(new SessionTokens(session.Id, session.RefreshToken, DateTimeOffset.MaxValue));
    }

    public Task EndAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        _sessions.RemoveAll(session => session.Id == sessionId);
        return Task.CompletedTask;
    }

    public Task EndAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        _sessions.RemoveAll(session => session.UserId == userId);
        return Task.CompletedTask;
    }

    public Task<RotatedSession?> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var index = _sessions.FindIndex(session => session.RefreshToken == refreshToken);
        if (index < 0)
        {
            return Task.FromResult<RotatedSession?>(null);
        }

        var current = _sessions[index];
        var rotated = current with { RefreshToken = NewToken() };
        _sessions[index] = rotated;
        return Task.FromResult<RotatedSession?>(
            new RotatedSession(rotated.UserId, new SessionTokens(rotated.Id, rotated.RefreshToken, DateTimeOffset.MaxValue)));
    }

    private string NewToken() => $"refresh-token-{++_issuedTokens}";

    internal sealed record StoredSession(Guid Id, Guid UserId, string RefreshToken);
}
