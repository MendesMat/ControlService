using ControlService.Application.Auth.SignIn;
using ControlService.Application.Auth.SignOut;

namespace ControlService.Application.Tests.Auth;

public class SignOutTests
{
    [Fact]
    public async Task Sign_out_ends_only_the_current_session()
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        var signIn = bed.CreateSignInHandler();
        await signIn.Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);
        await signIn.Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);
        var (ending, staying) = (bed.Sessions.SessionsOf(user.Id)[0], bed.Sessions.SessionsOf(user.Id)[1]);

        var result = await bed.CreateSignOutHandler()
            .Handle(new SignOutCommand(ending.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        bed.Sessions.SessionsOf(user.Id).ShouldBe([staying]);
    }
}
