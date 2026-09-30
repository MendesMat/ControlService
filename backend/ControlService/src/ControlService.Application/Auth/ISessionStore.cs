namespace ControlService.Application.Auth;

/// <summary>Sessions are refresh tokens stored hashed (AUTH-19, ADR-0019).</summary>
public interface ISessionStore
{
    Task<SessionTokens> StartAsync(Guid userId, CancellationToken cancellationToken);

    Task EndAsync(Guid sessionId, CancellationToken cancellationToken);

    Task EndAllAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Replaces a valid refresh token with a new one, or returns null when the token is
    /// unknown or has expired. Two calls with the same token never both succeed (ADR-0019).</summary>
    Task<RotatedSession?> RotateAsync(string refreshToken, CancellationToken cancellationToken);
}
