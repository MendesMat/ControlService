using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Domain.Tests.Access;

public class EffectiveAccessTests
{
    [Fact]
    public void No_profiles_results_in_denied_on_every_screen() // PERM-05
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;

        var level = EffectiveAccess.GetLevel([], users);

        level.ShouldBe(AccessLevel.Denied);
    }
}
