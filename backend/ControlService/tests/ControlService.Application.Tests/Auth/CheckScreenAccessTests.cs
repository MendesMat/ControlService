using ControlService.Application.Auth;
using ControlService.Application.Auth.CheckScreenAccess;
using ControlService.Domain.Access;

namespace ControlService.Application.Tests.Auth;

public class CheckScreenAccessTests
{
    // #9-T20
    [Fact]
    public async Task A_user_that_no_longer_exists_is_refused_as_session_expired() // AUTH-27
    {
        var bed = new AuthTestBed();
        var query = new CheckScreenAccessQuery(Guid.CreateVersion7(), ScreenKey.Create(ScreenKeys.Users).Value, AccessLevel.Reader);

        var result = await bed.CreateCheckScreenAccessHandler().Handle(query, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.SessionExpired);
    }
}
