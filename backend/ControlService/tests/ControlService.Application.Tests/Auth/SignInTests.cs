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
}
