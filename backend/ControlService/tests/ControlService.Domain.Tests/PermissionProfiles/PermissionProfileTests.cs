using ControlService.Domain.Access;
using ControlService.Domain.Common;
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

    [Fact]
    public void SetLevel_changes_only_that_screen() // ADR-0006
    {
        var profile = PermissionProfile.Create();
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var customers = ScreenKey.Create(ScreenKeys.Customers).Value;

        profile.SetLevel(users, AccessLevel.Editor);

        profile.GetLevel(users).ShouldBe(AccessLevel.Editor);
        profile.GetLevel(customers).ShouldBe(AccessLevel.Denied);
    }

    [Fact]
    public void Manager_profile_is_a_system_record() // PERM-22
    {
        var profile = PermissionProfile.CreateManagerProfile();

        profile.Id.ShouldBe(SystemIds.ManagerProfile);
        profile.IsSystem.ShouldBeTrue();
    }
}
