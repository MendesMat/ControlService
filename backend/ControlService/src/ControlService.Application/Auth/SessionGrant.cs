namespace ControlService.Application.Auth;

/// <summary>What a sign-in, a refresh or a password change grants. The endpoint sends the first
/// three in the body and the refresh token in its cookie.</summary>
public sealed record SessionGrant(
    string AccessToken,
    int ExpiresIn,
    bool MustChangePassword,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt)
{
    public static SessionGrant From(AccessToken accessToken, SessionTokens session, bool mustChangePassword) =>
        new(accessToken.Value, accessToken.ExpiresInSeconds, mustChangePassword, session.RefreshToken, session.ExpiresAt);
}
