namespace ControlService.Application.Auth;

/// <summary>Passwords, lockout and the mandatory-change flag. Credentials are not part of the user
/// record (AUTH-19); lockout counting lives behind this interface (ADR-0019).</summary>
public interface ICredentialStore
{
    Task<CredentialCheck> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    /// <summary>True while the account still has its initial password (AUTH-14).</summary>
    Task<bool> MustChangePasswordAsync(Guid userId, CancellationToken cancellationToken);
}
