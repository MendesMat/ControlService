namespace ControlService.Application.Auth;

/// <summary>Passwords, lockout and the mandatory-change flag. Credentials are not part of the user
/// record (AUTH-19); lockout counting lives behind this interface.</summary>
public interface ICredentialStore
{
    Task<CredentialCheck> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    /// <summary>Compares without counting a failure toward the lockout: it is not a sign-in attempt.</summary>
    Task<bool> IsCurrentPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    /// <summary>Sets a new password and clears the mandatory-change flag.</summary>
    Task ReplacePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);

    /// <summary>True while the account still has its initial password (AUTH-14).</summary>
    Task<bool> MustChangePasswordAsync(Guid userId, CancellationToken cancellationToken);
}
