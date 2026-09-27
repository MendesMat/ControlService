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

    [Fact]
    public void One_profile_gives_its_level() // PERM-05
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var profile = PermissionProfile.Create();
        profile.SetLevel(users, AccessLevel.Editor);

        var level = EffectiveAccess.GetLevel([profile], users);

        level.ShouldBe(AccessLevel.Editor);
    }

    [Fact]
    public void Screen_missing_from_a_profile_counts_as_denied() // PERM-05
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var customers = ScreenKey.Create(ScreenKeys.Customers).Value;
        var profile = PermissionProfile.Create();
        profile.SetLevel(users, AccessLevel.Editor);

        var level = EffectiveAccess.GetLevel([profile], customers);

        level.ShouldBe(AccessLevel.Denied);
    }

    [Fact]
    public void Denied_in_one_profile_and_editor_in_another_results_in_editor() // PERM-05, PERM-07
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var deniedProfile = PermissionProfile.Create();
        var editorProfile = PermissionProfile.Create();
        editorProfile.SetLevel(users, AccessLevel.Editor);

        var level = EffectiveAccess.GetLevel([deniedProfile, editorProfile], users);

        level.ShouldBe(AccessLevel.Editor);
    }
}
