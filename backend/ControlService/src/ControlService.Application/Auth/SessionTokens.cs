namespace ControlService.Application.Auth;

/// <summary>A session just started or rotated: its id, the plain refresh token (returned once, for
/// the cookie) and when the token stops working if it is not used.</summary>
public sealed record SessionTokens(Guid SessionId, string RefreshToken, DateTimeOffset ExpiresAt);
