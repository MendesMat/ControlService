using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Domain.Tests.PermissionProfiles;

public class PermissionProfileTests
{
    [Fact]
    public void New_profile_has_every_screen_denied() // PERM-18
    {
        var profile = PermissionProfile.Create();

        foreach (var screen in ScreenKeys.All)
        {
            profile.GetLevel(ScreenKey.Create(screen).Value).ShouldBe(AccessLevel.Denied);
        }
    }
}
