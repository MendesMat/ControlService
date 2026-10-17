using ControlService.Application.Auth;
using ControlService.Application.Auth.CheckAccount;

namespace ControlService.Application.Tests.Auth;

public class CheckAccountTests
{
    // #9-T01
    [Fact]
    public async Task A_user_that_no_longer_exists_is_refused_as_session_expired() // AUTH-27
    {
        var bed = new AuthTestBed();

        var result = await bed.CreateCheckAccountHandler()
            .Handle(new CheckAccountQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.SessionExpired);
    }

}
