namespace ControlService.Application.Auth.RefreshSession;

/// <summary>`RefreshToken` is what the cookie carried, or an empty string when there was no cookie.</summary>
public sealed record RefreshSessionCommand(string RefreshToken);
