namespace ControlService.Application.Auth.ChangePassword;

/// <summary>`UserId` comes from the `sub` claim of the access token. Passwords are never trimmed:
/// a password is stored only as a hash (CNV-03 is about stored text).</summary>
public sealed record ChangePasswordCommand(Guid UserId, string Password, string PasswordConfirmation);
