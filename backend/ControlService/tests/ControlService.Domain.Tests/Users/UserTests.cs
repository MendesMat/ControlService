using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void New_user_is_pending() // AUTH-02
    {
        var user = User.Create();

        user.Status.ShouldBe(UserStatus.Pending);
    }
}
