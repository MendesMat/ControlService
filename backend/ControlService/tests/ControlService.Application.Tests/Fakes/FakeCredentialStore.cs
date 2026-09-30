using ControlService.Application.Auth;

namespace ControlService.Application.Tests.Fakes;

/// <summary>Holds one password per user. Lockout counting is Identity's job and is tested against
/// the real store; here a test scripts the outcome it needs.</summary>
internal sealed class FakeCredentialStore : ICredentialStore
{
    private readonly Dictionary<Guid, string> _passwords = [];
    private readonly Dictionary<Guid, DateTimeOffset> _lockouts = [];
    private readonly HashSet<Guid> _mustChange = [];

    public void SetPassword(Guid userId, string password) => _passwords[userId] = password;

    public void RequirePasswordChange(Guid userId) => _mustChange.Add(userId);

    public void LockOutUntil(Guid userId, DateTimeOffset lockoutEnd) => _lockouts[userId] = lockoutEnd;

    public Task<CredentialCheck> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        if (_lockouts.TryGetValue(userId, out var lockoutEnd))
        {
            return Task.FromResult(CredentialCheck.LockedOut(lockoutEnd));
        }

        return Task.FromResult(_passwords.TryGetValue(userId, out var stored) && stored == password
            ? CredentialCheck.Succeeded
            : CredentialCheck.Failed);
    }

    public Task ReplacePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        _passwords[userId] = newPassword;
        _mustChange.Remove(userId);
        return Task.CompletedTask;
    }

    public Task<bool> IsCurrentPasswordAsync(Guid userId, string password, CancellationToken cancellationToken) =>
        Task.FromResult(_passwords.TryGetValue(userId, out var stored) && stored == password);

    public Task<bool> MustChangePasswordAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(_mustChange.Contains(userId));
}
