using Microsoft.AspNetCore.Identity;

namespace ControlService.Infrastructure.Auth;

/// <summary>The person's credential: password hash, lockout counter and security stamp, kept apart
/// from the user record (AUTH-19). `Id` is the user's id and `UserName` is that id too, so nothing
/// needs syncing when a login changes; Identity's e-mail and phone columns stay empty.</summary>
public sealed class UserCredential : IdentityUser<Guid>
{
    /// <summary>True while the account still has a temporary password (AUTH-14).</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>A credential with a password that must be replaced at the first sign-in (AUTH-14): the Admin's
    /// initial password and the temporary password of a new user. Not saved; the caller adds it to the context.</summary>
    public static UserCredential CreateWithTemporaryPassword(Guid userId, string password, IPasswordHasher<UserCredential> hasher)
    {
        var id = userId.ToString();
        var credential = new UserCredential
        {
            Id = userId,
            UserName = id,
            NormalizedUserName = id.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
            MustChangePassword = true,
        };
        credential.PasswordHash = hasher.HashPassword(credential, password);
        return credential;
    }
}
