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

    [Fact]
    public void Highest_level_wins_screen_by_screen() // PERM-05, the Carla example in permission-profiles.md
    {
        var customers = ScreenKey.Create(ScreenKeys.Customers).Value;
        var accountsReceivable = ScreenKey.Create(ScreenKeys.AccountsReceivable).Value;
        var users = ScreenKey.Create(ScreenKeys.Users).Value;

        var vendedor = PermissionProfile.Create();
        vendedor.SetLevel(customers, AccessLevel.Editor);
        vendedor.SetLevel(accountsReceivable, AccessLevel.Reader);

        var financeiro = PermissionProfile.Create();
        financeiro.SetLevel(customers, AccessLevel.Reader);
        financeiro.SetLevel(accountsReceivable, AccessLevel.Manager);

        PermissionProfile[] carlasProfiles = [vendedor, financeiro];

        EffectiveAccess.GetLevel(carlasProfiles, customers).ShouldBe(AccessLevel.Editor);
        EffectiveAccess.GetLevel(carlasProfiles, accountsReceivable).ShouldBe(AccessLevel.Manager);
        EffectiveAccess.GetLevel(carlasProfiles, users).ShouldBe(AccessLevel.Denied);
    }

    [Fact]
    public void Unknown_profile_id_is_ignored() // PERM-05
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var known = PermissionProfile.Create();
        known.SetLevel(users, AccessLevel.Editor);
        var profilesById = new Dictionary<Guid, PermissionProfile> { [known.Id] = known };
        Guid[] profileIds = [known.Id, Guid.CreateVersion7()];

        var level = EffectiveAccess.GetLevel(profileIds, profilesById, users);

        level.ShouldBe(AccessLevel.Editor);
    }
}
