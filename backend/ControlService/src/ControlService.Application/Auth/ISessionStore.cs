namespace ControlService.Application.Auth;

/// <summary>Sessions are refresh tokens stored hashed (AUTH-19, ADR-0019).</summary>
public interface ISessionStore
{
    Task<SessionTokens> StartAsync(Guid userId, CancellationToken cancellationToken);
}
