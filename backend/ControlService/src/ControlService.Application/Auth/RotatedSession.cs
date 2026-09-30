namespace ControlService.Application.Auth;

/// <summary>A refresh token that was valid and has just been replaced by a new one (AUTH-17).</summary>
public sealed record RotatedSession(Guid UserId, SessionTokens Tokens);
