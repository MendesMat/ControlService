namespace ControlService.Application.Auth;

/// <summary>The configured values that appear in user-facing messages (AUTH-22, AUTH-28). Infrastructure
/// fills it from the `Auth` configuration section.</summary>
public sealed record AuthSettings(int PasswordMinLength, int LockoutMinutes);
