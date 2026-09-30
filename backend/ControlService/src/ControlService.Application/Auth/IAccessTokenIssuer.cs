namespace ControlService.Application.Auth;

public interface IAccessTokenIssuer
{
    AccessToken Issue(Guid userId, Guid sessionId, bool mustChangePassword);
}

/// <summary>The signed access token and how many seconds it lasts, so a wrong clock on the person's
/// computer does not break the refresh decision (D1).</summary>
public sealed record AccessToken(string Value, int ExpiresInSeconds);
