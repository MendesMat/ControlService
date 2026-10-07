using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControlService.Infrastructure.Auth;

/// <summary>Identity's lockout reads `DateTimeOffset.UtcNow` directly, which tests cannot control.
/// This manager overrides only the two virtual methods that compare or set the lockout end so they
/// use the application's <see cref="TimeProvider"/>; the counting and the threshold are Identity's
/// own logic, copied unchanged.</summary>
public sealed class CredentialUserManager(
    IUserStore<UserCredential> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<UserCredential> passwordHasher,
    IEnumerable<IUserValidator<UserCredential>> userValidators,
    IEnumerable<IPasswordValidator<UserCredential>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<UserCredential>> logger,
    TimeProvider timeProvider)
    : UserManager<UserCredential>(
        store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
{
    public override async Task<bool> IsLockedOutAsync(UserCredential user)
    {
        if (!await GetLockoutEnabledAsync(user))
        {
            return false;
        }

        // Locked while the end is still ahead: at exactly the end the wait is over and Retry-After is 0.
        return await GetLockoutEndDateAsync(user) > timeProvider.GetUtcNow();
    }

    public override async Task<IdentityResult> AccessFailedAsync(UserCredential user)
    {
        var lockoutStore = (IUserLockoutStore<UserCredential>)Store;
        var count = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken);
        if (count < Options.Lockout.MaxFailedAccessAttempts)
        {
            return await UpdateAsync(user);
        }

        // Reaching the threshold locks the login and starts counting again (AUTH-08).
        await lockoutStore.SetLockoutEndDateAsync(
            user, timeProvider.GetUtcNow().Add(Options.Lockout.DefaultLockoutTimeSpan), CancellationToken);
        await lockoutStore.ResetAccessFailedCountAsync(user, CancellationToken);
        return await UpdateAsync(user);
    }
}
