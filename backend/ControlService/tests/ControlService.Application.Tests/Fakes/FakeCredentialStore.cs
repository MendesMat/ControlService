using ControlService.Application.Auth;

namespace ControlService.Application.Tests.Fakes;

/// <summary>Holds one password per user. Lockout counting is Identity's job and is tested against
/// the real store; here a test scripts the outcome it needs.</summary>
internal sealed class FakeCredentialStore : ICredentialStore
{
    private readonly Dictionary<Guid, string> _passwords = [];

    public void SetPassword(Guid userId, string password) => _passwords[userId] = password;

    public Task<CredentialCheck> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken) =>
        Task.FromResult(_passwords.TryGetValue(userId, out var stored) && stored == password
            ? CredentialCheck.Succeeded
            : CredentialCheck.Failed);
}
