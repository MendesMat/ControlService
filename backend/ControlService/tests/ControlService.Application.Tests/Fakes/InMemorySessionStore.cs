using ControlService.Application.Auth;

namespace ControlService.Application.Tests.Fakes;

internal sealed class InMemorySessionStore : ISessionStore
{
    private readonly List<StoredSession> _sessions = [];

    public IReadOnlyList<StoredSession> SessionsOf(Guid userId) => _sessions.Where(session => session.UserId == userId).ToArray();

    public Task<SessionTokens> StartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var session = new StoredSession(Guid.CreateVersion7(), userId, $"refresh-token-{_sessions.Count + 1}");
        _sessions.Add(session);
        return Task.FromResult(new SessionTokens(session.Id, session.RefreshToken, DateTimeOffset.MaxValue));
    }

    internal sealed record StoredSession(Guid Id, Guid UserId, string RefreshToken);
}
