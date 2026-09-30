using ControlService.Application.Auth;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Tests.Fakes;

namespace ControlService.Application.Tests.Auth;

public class SignInTests
{
    [Fact]
    public async Task Sign_in_with_the_right_password_returns_a_token_and_starts_a_session() // AUTH-07
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var session = bed.Sessions.SessionsOf(user.Id).ShouldHaveSingleItem();
        result.Value.AccessToken.ShouldBe(FakeAccessTokenIssuer.TokenFor(user.Id, session.Id, mustChangePassword: false));
        result.Value.ExpiresIn.ShouldBe(900);
        result.Value.MustChangePassword.ShouldBeFalse();
        result.Value.RefreshToken.ShouldBe(session.RefreshToken);
    }

    [Fact]
    public async Task Unknown_login_is_refused_as_invalid_credentials() // AUTH-08
    {
        var bed = new AuthTestBed();

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("nobody", "any-password"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("invalid_credentials");
        result.Error.Message.ShouldBe("Login ou senha incorretos.");
    }

    [Fact]
    public async Task Login_outside_the_login_format_is_refused_as_invalid_credentials() // AUTH-08
    {
        var bed = new AuthTestBed();

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("a", "any-password"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Wrong_password_is_refused_with_the_same_error_as_an_unknown_login() // AUTH-08
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "outra-senha"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.InvalidCredentials);
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Pending_account_is_refused_as_invalid_credentials() // AUTH-02, AUTH-08
    {
        var bed = new AuthTestBed();
        var user = bed.AddPendingUser("bruno.lima");

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("bruno.lima", "any-password"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.InvalidCredentials);
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Locked_out_login_is_refused_even_with_the_right_password() // AUTH-08
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        bed.Credentials.LockOutUntil(user.Id, bed.Clock.GetUtcNow().AddMinutes(10));

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("locked_out");
        result.Error.Message.ShouldBe("Muitas tentativas sem sucesso. Aguarde 15 minutos e tente de novo.");
        result.Error.Details.ShouldNotBeNull();
        result.Error.Details["retryAfterSeconds"].ShouldBe(600);
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Lockout_message_uses_the_configured_minutes() // AUTH-28
    {
        var bed = new AuthTestBed(new AuthSettings(PasswordMinLength: 8, LockoutMinutes: 30));
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        bed.Credentials.LockOutUntil(user.Id, bed.Clock.GetUtcNow().AddMinutes(30));

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Muitas tentativas sem sucesso. Aguarde 30 minutos e tente de novo.");
    }

    [Fact]
    public async Task Deactivated_account_with_the_right_password_is_refused() // AUTH-09
    {
        var bed = new AuthTestBed();
        var user = bed.AddInactiveUser("carla.dias", "senha-da-carla");

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("carla.dias", "senha-da-carla"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("account_inactive");
        result.Error.Message.ShouldBe("Este acesso está desativado. Fale com o responsável pelo sistema.");
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Deactivated_account_with_a_wrong_password_gets_invalid_credentials() // AUTH-08, AUTH-09
    {
        var bed = new AuthTestBed();
        bed.AddInactiveUser("carla.dias", "senha-da-carla");

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("carla.dias", "outra-senha"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Account_with_a_mandatory_password_change_gets_a_restricted_token() // AUTH-14, ADR-0032
    {
        var bed = new AuthTestBed();
        var admin = bed.AddActiveUser("admin", "senha-inicial");
        bed.Credentials.RequirePasswordChange(admin.Id);

        var result = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("admin", "senha-inicial"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var session = bed.Sessions.SessionsOf(admin.Id).ShouldHaveSingleItem();
        result.Value.MustChangePassword.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe(FakeAccessTokenIssuer.TokenFor(admin.Id, session.Id, mustChangePassword: true));
    }
}
