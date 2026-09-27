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

    [Fact]
    public void New_user_gets_a_version_7_id() // CNV-04, ADR-0012
    {
        var user = User.Create();

        user.Id.ShouldNotBe(Guid.Empty);
        var versionNibble = user.Id.ToByteArray()[7] >> 4;
        versionNibble.ShouldBe(7);
    }
}
