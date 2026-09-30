using ControlService.Application.Auth;
using ControlService.Application.Auth.RefreshSession;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Tests.Fakes;

namespace ControlService.Application.Tests.Auth;

public class RefreshSessionTests
{
    [Fact]
    public async Task Refresh_with_a_valid_token_rotates_it_and_issues_a_new_access_token() // AUTH-17
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        var signedIn = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

        var result = await bed.CreateRefreshSessionHandler()
            .Handle(new RefreshSessionCommand(signedIn.Value.RefreshToken), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var session = bed.Sessions.SessionsOf(user.Id).ShouldHaveSingleItem();
        result.Value.RefreshToken.ShouldNotBe(signedIn.Value.RefreshToken);
        result.Value.RefreshToken.ShouldBe(session.RefreshToken);
        result.Value.AccessToken.ShouldBe(FakeAccessTokenIssuer.TokenFor(user.Id, session.Id, mustChangePassword: false));
        result.Value.ExpiresIn.ShouldBe(900);
        result.Value.MustChangePassword.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Refresh_with_an_unknown_or_expired_token_is_refused_as_session_expired(bool expired) // AUTH-17, AUTH-27
    {
        var bed = new AuthTestBed();
        bed.AddActiveUser("ana.souza", "senha-da-ana");
        var signedIn = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);
        var token = expired ? signedIn.Value.RefreshToken : "unknown-token";
        bed.Sessions.Expire(signedIn.Value.RefreshToken);

        var result = await bed.CreateRefreshSessionHandler()
            .Handle(new RefreshSessionCommand(token), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("session_expired");
        result.Error.Message.ShouldBe("Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas.");
    }

    [Fact]
    public async Task Refresh_of_a_deactivated_account_is_refused_and_ends_the_session() // AUTH-18, AUTH-27
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        var signedIn = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);
        user.Deactivate(Guid.CreateVersion7(), bed.Clock.GetUtcNow());

        var result = await bed.CreateRefreshSessionHandler()
            .Handle(new RefreshSessionCommand(signedIn.Value.RefreshToken), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("account_inactive");
        result.Error.Message.ShouldBe("Este acesso está desativado. Fale com o responsável pelo sistema.");
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Refresh_keeps_the_mandatory_password_change() // AUTH-14
    {
        var bed = new AuthTestBed();
        var admin = bed.AddActiveUser("admin", "senha-inicial");
        bed.Credentials.RequirePasswordChange(admin.Id);
        var signedIn = await bed.CreateSignInHandler()
            .Handle(new SignInCommand("admin", "senha-inicial"), TestContext.Current.CancellationToken);

        var result = await bed.CreateRefreshSessionHandler()
            .Handle(new RefreshSessionCommand(signedIn.Value.RefreshToken), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var session = bed.Sessions.SessionsOf(admin.Id).ShouldHaveSingleItem();
        result.Value.MustChangePassword.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe(FakeAccessTokenIssuer.TokenFor(admin.Id, session.Id, mustChangePassword: true));
    }

    [Fact]
    public async Task Refresh_of_a_session_whose_user_no_longer_exists_is_refused_as_session_expired() // AUTH-17
    {
        var bed = new AuthTestBed();
        var orphan = await bed.Sessions.StartAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        var result = await bed.CreateRefreshSessionHandler()
            .Handle(new RefreshSessionCommand(orphan.RefreshToken), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.SessionExpired);
    }
}
