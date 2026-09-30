using ControlService.Application.Auth;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Tests.Fakes;
using ControlService.Domain.Users;

namespace ControlService.Application.Tests.Auth;

/// <summary>Builds the fakes and the handlers of the authentication use cases, and the users the
/// tests sign in as, so a constructor change touches one file (plan of #7).</summary>
internal sealed class AuthTestBed(AuthSettings? settings = null)
{
    public AuthSettings Settings { get; } = settings ?? new AuthSettings(PasswordMinLength: 8, LockoutMinutes: 15);

    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));

    public InMemoryUserRepository Users { get; } = new();

    public FakeCredentialStore Credentials { get; } = new();

    public InMemorySessionStore Sessions { get; } = new();

    public FakeAccessTokenIssuer Tokens { get; } = new();

    public SignInHandler CreateSignInHandler() => new(Users, Credentials, Sessions, Tokens, Settings, Clock);

    public User AddPendingUser(string login)
    {
        var user = User.Create(
            Login.Create(login).Value, EmailAddress.Create($"{login}@example.com").Value, login, login);
        Users.Add(user);
        return user;
    }

    public User AddActiveUser(string login, string password)
    {
        var user = AddPendingUser(login);
        user.Activate(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));
        Credentials.SetPassword(user.Id, password);
        return user;
    }

    public User AddInactiveUser(string login, string password)
    {
        var user = AddActiveUser(login, password);
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 2, 8, 0, 0, TimeSpan.Zero));
        return user;
    }
}
