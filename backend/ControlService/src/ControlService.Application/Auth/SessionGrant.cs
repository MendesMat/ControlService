namespace ControlService.Application.Auth;

/// <summary>What a sign-in, a refresh or a password change grants. The endpoint sends the first
/// three in the body and the refresh token in its cookie (D1).</summary>
public sealed record SessionGrant(
    string AccessToken,
    int ExpiresIn,
    bool MustChangePassword,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
