using ControlService.Domain.Access;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Domain.Tests.PermissionProfiles;

public class PermissionProfileTests
{
    [Fact]
    public void Created_profile_keeps_name_and_description_without_surrounding_spaces() // CNV-03
    {
        var profile = PermissionProfile.Create(" Vendedor ", " Equipe comercial ");

        profile.Name.ShouldBe("Vendedor");
        profile.Description.ShouldBe("Equipe comercial");
    }

    [Fact]
    public void Normalized_profile_name_ignores_case_accents_and_surrounding_spaces() // CNV-09, PERM-17
    {
        var profile = PermissionProfile.Create("Fínanceiro ", "Equipe financeira");

        profile.NormalizedName.ShouldBe("financeiro");
    }

    [Fact]
    public void New_profile_has_every_screen_denied() // PERM-18
    {
        var profile = TestData.NewProfile();

        foreach (var screen in ScreenKeys.All)
        {
            profile.GetLevel(ScreenKey.Create(screen).Value).ShouldBe(AccessLevel.Denied);
        }
    }

    [Fact]
    public void Setting_a_level_again_replaces_the_previous_one() // PERM-01
    {
        var profile = TestData.NewProfile();
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        profile.SetLevel(users, AccessLevel.Editor);

        profile.SetLevel(users, AccessLevel.Reader);

        profile.Levels.ShouldBe([new ScreenLevel(users, AccessLevel.Reader)]);
    }

    [Fact]
    public void Setting_a_level_to_denied_removes_the_screen_from_the_stored_levels() // PERM-05, ADR-0011
    {
        var profile = TestData.NewProfile();
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        profile.SetLevel(users, AccessLevel.Editor);

        profile.SetLevel(users, AccessLevel.Denied);

        profile.Levels.ShouldBeEmpty();
        profile.GetLevel(users).ShouldBe(AccessLevel.Denied);
    }

    [Fact]
    public void SetLevel_changes_only_that_screen() // ADR-0006
    {
        var profile = TestData.NewProfile();
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var customers = ScreenKey.Create(ScreenKeys.Customers).Value;

        var result = profile.SetLevel(users, AccessLevel.Editor);

        result.IsSuccess.ShouldBeTrue();
        profile.GetLevel(users).ShouldBe(AccessLevel.Editor);
        profile.GetLevel(customers).ShouldBe(AccessLevel.Denied);
    }

    [Fact]
    public void Manager_profile_has_the_fixed_system_values() // PERM-22, PERM-23
    {
        var profile = PermissionProfile.CreateManagerProfile();

        profile.Id.ShouldBe(SystemIds.ManagerProfile);
        profile.Name.ShouldBe("Gerenciador");
        profile.Description.ShouldBe("Acesso total a todas as telas do sistema.");
        profile.IsSystem.ShouldBeTrue();
        profile.Levels.ShouldBeEmpty();
    }

    [Fact]
    public void Manager_profile_has_manager_on_every_screen_of_the_catalog() // PERM-23
    {
        var profile = PermissionProfile.CreateManagerProfile();

        foreach (var screen in ScreenKeys.All)
        {
            profile.GetLevel(ScreenKey.Create(screen).Value).ShouldBe(AccessLevel.Manager);
        }
    }

    [Fact]
    public void SetLevel_on_the_manager_profile_is_refused() // PERM-22, PERM-25
    {
        var profile = PermissionProfile.CreateManagerProfile();
        var users = ScreenKey.Create(ScreenKeys.Users).Value;

        var result = profile.SetLevel(users, AccessLevel.Editor);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("system_record");
        result.Error.Message.ShouldBe("O perfil Gerenciador é do sistema e não pode ser alterado nem excluído.");
        profile.GetLevel(users).ShouldBe(AccessLevel.Manager);
    }

    [Fact]
    public void Regular_profile_can_be_deleted() // PERM-20
    {
        var profile = TestData.NewProfile();

        var result = profile.Delete();

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Manager_profile_cannot_be_deleted() // PERM-22
    {
        var profile = PermissionProfile.CreateManagerProfile();

        var result = profile.Delete();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("system_record");
        result.Error.Message.ShouldBe("O perfil Gerenciador é do sistema e não pode ser alterado nem excluído.");
    }
}
