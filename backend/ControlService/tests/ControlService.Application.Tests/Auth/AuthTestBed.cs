using ControlService.Application.Auth;
using ControlService.Application.Auth.ChangePassword;
using ControlService.Application.Auth.GetMe;
using ControlService.Application.Auth.RefreshSession;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Auth.SignOut;
using ControlService.Application.Common;
using ControlService.Application.Tests.Fakes;
using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;

namespace ControlService.Application.Tests.Auth;

/// <summary>Builds the fakes and the handlers of the authentication use cases, and the users the
/// tests sign in as, so a constructor change touches one file (plan of #7).</summary>
internal sealed class AuthTestBed(AuthSettings? settings = null)
{
    public AuthSettings Settings { get; } = settings ?? new AuthSettings(PasswordMinLength: 8, LockoutMinutes: 15);

    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));

    public InMemoryUserRepository Users { get; } = new();

    public InMemoryPermissionProfileRepository Profiles { get; } = new();

    public FakeCredentialStore Credentials { get; } = new();

    public InMemorySessionStore Sessions { get; } = new();

    public FakeAccessTokenIssuer Tokens { get; } = new();

    public FakeUnitOfWork UnitOfWork { get; } = new();

    public SignInHandler CreateSignInHandler() => new(Users, Credentials, Sessions, Tokens, Settings, Clock);

    public RefreshSessionHandler CreateRefreshSessionHandler() => new(Users, Credentials, Sessions, Tokens);

    public SignOutHandler CreateSignOutHandler() => new(Sessions);

    public ValidatingCommandHandler<ChangePasswordCommand, SessionGrant> CreateChangePasswordHandler() =>
        new(new ChangePasswordHandler(Users, Credentials, Sessions, Tokens, UnitOfWork, Clock), new ChangePasswordValidator(Settings));

    public GetMeHandler CreateGetMeHandler() => new(Users, Profiles);

    /// <summary>Gives the user a new profile with the given levels.</summary>
    public PermissionProfile AddProfile(User user, params (string Screen, AccessLevel Level)[] levels)
    {
        var profile = PermissionProfile.Create($"Profile {Guid.CreateVersion7()}", "Test profile");
        foreach (var (screen, level) in levels)
        {
            profile.SetLevel(ScreenKey.Create(screen).Value, level);
        }

        Profiles.Add(profile);
        user.AssignProfiles([.. user.ProfileIds, profile.Id]);
        return profile;
    }

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

    /// <summary>The Admin as the seeder creates it, with the initial password that must be replaced (AUTH-13).</summary>
    public User AddAdminWithInitialPassword(string initialPassword)
    {
        var admin = User.CreateAdmin(EmailAddress.Create("admin@example.com").Value);
        Users.Add(admin);
        Credentials.SetPassword(admin.Id, initialPassword);
        Credentials.RequirePasswordChange(admin.Id);
        return admin;
    }

    public User AddInactiveUser(string login, string password)
    {
        var user = AddActiveUser(login, password);
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 2, 8, 0, 0, TimeSpan.Zero));
        return user;
    }
}
