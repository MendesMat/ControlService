using Microsoft.AspNetCore.Identity;

namespace ControlService.Infrastructure.Auth;

/// <summary>The person's credential: password hash, lockout counter and security stamp, kept apart
/// from the user record (AUTH-19). `Id` is the user's id and `UserName` is that id too, so nothing
/// needs syncing when a login changes; Identity's e-mail and phone columns stay empty (D12).</summary>
public sealed class UserCredential : IdentityUser<Guid>
{
    /// <summary>True while the account still has its initial password (AUTH-14).</summary>
    public bool MustChangePassword { get; set; }
}
