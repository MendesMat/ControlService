using ControlService.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace ControlService.Infrastructure.Auth;

/// <summary>Passwords, lockout and the mandatory-change flag on top of Identity's UserManager, without
/// SignInManager. Lockout is checked before the password, and an attempt made while locked out is
/// not counted.</summary>
public sealed class CredentialStore(UserManager<UserCredential> users) : ICredentialStore
{
    public async Task<CredentialCheck> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var credential = await FindAsync(userId);
        if (credential is null)
        {
            return CredentialCheck.Failed;
        }

        if (await users.IsLockedOutAsync(credential))
        {
            return await LockedOutAsync(credential);
        }

        if (await users.CheckPasswordAsync(credential, password))
        {
            await users.ResetAccessFailedCountAsync(credential);
            return CredentialCheck.Succeeded;
        }

        await users.AccessFailedAsync(credential);
        return await users.IsLockedOutAsync(credential) ? await LockedOutAsync(credential) : CredentialCheck.Failed;
    }

    public async Task<bool> IsCurrentPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var credential = await FindAsync(userId);
        return credential is not null && await users.CheckPasswordAsync(credential, password);
    }

    public async Task<bool> MustChangePasswordAsync(Guid userId, CancellationToken cancellationToken) =>
        (await FindAsync(userId))?.MustChangePassword ?? false;

    public async Task ReplacePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        var credential = await FindAsync(userId) ?? throw new InvalidOperationException("The user has no credential.");
        credential.PasswordHash = users.PasswordHasher.HashPassword(credential, newPassword);
        credential.MustChangePassword = false;
        // A new password invalidates whatever the old one had granted (security stamp).
        credential.SecurityStamp = Guid.NewGuid().ToString();

        var updated = await users.UpdateAsync(credential);
        if (!updated.Succeeded)
        {
            throw new InvalidOperationException("The credential could not be updated.");
        }
    }

    private async Task<UserCredential?> FindAsync(Guid userId) => await users.FindByIdAsync(userId.ToString());

    private async Task<CredentialCheck> LockedOutAsync(UserCredential credential) =>
        CredentialCheck.LockedOut((await users.GetLockoutEndDateAsync(credential))!.Value);
}
