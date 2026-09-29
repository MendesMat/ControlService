using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Created_user_keeps_login_email_display_name_and_full_name() // USR-01, USR-02, USR-04, USR-07
    {
        var login = Login.Create("ana.souza").Value;
        var email = EmailAddress.Create("ana.souza@example.com").Value;

        var user = User.Create(login, email, "Ana Souza", "Ana Paula Souza");

        user.Login.Value.ShouldBe("ana.souza");
        user.Email.Value.ShouldBe("ana.souza@example.com");
        user.DisplayName.ShouldBe("Ana Souza");
        user.FullName.ShouldBe("Ana Paula Souza");
        user.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void Created_user_stores_names_without_surrounding_spaces() // CNV-03
    {
        var login = Login.Create("ana.souza").Value;
        var email = EmailAddress.Create("ana.souza@example.com").Value;

        var user = User.Create(login, email, "  Ana Souza  ", "  Ana Paula Souza  ");

        user.DisplayName.ShouldBe("Ana Souza");
        user.FullName.ShouldBe("Ana Paula Souza");
    }

    [Fact]
    public void Normalized_display_name_ignores_case_accents_and_surrounding_spaces() // CNV-09, USR-03
    {
        var login = Login.Create("ana.souza").Value;
        var email = EmailAddress.Create("ana.souza@example.com").Value;

        var user = User.Create(login, email, " Ána SOUZA ", "Ana Paula Souza");

        user.NormalizedDisplayName.ShouldBe("ana souza");
    }

    [Fact]
    public void New_user_is_pending() // AUTH-02
    {
        var user = TestData.NewUser();

        user.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void New_user_gets_a_version_7_id() // CNV-04, ADR-0012
    {
        var user = TestData.NewUser();

        user.Id.ShouldNotBe(Guid.Empty);
        var versionNibble = user.Id.ToByteArray()[7] >> 4;
        versionNibble.ShouldBe(7);
    }

    [Fact]
    public void Activating_a_pending_user_makes_it_active_with_the_activation_date() // AUTH-02
    {
        var user = TestData.NewUser();
        var now = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);

        var result = user.Activate(now);

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Active);
        user.ActivatedAt.ShouldBe(now);
    }

    [Fact]
    public void Activating_an_active_user_is_refused_as_invalid_link() // AUTH-23
    {
        var user = TestData.NewUser();
        user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));

        var result = user.Activate(new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("link_invalid");
        result.Error.Message.ShouldBe(
            "Este link não vale mais. Peça a quem cadastrou você para reenviar o acesso.");
    }

    [Fact]
    public void Activating_an_inactive_user_is_refused_as_invalid_link() // AUTH-23, AUTH-21
    {
        var user = TestData.NewUser();
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero));

        var result = user.Activate(new DateTimeOffset(2026, 9, 21, 9, 0, 0, TimeSpan.Zero));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("link_invalid");
        user.Status.ShouldBe(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivating_an_active_user_records_status_date_and_author() // USR-16, USR-17
    {
        var user = TestData.NewUser();
        user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));
        var deactivatedBy = Guid.CreateVersion7();
        var deactivatedAt = new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero);

        var result = user.Deactivate(deactivatedBy, deactivatedAt);

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Inactive);
        user.DeactivatedAt.ShouldBe(deactivatedAt);
        user.DeactivatedBy.ShouldBe(deactivatedBy);
    }

    [Fact]
    public void Deactivating_a_pending_user_makes_it_inactive() // USR-17
    {
        var user = TestData.NewUser();

        var result = user.Deactivate(Guid.CreateVersion7(), DateTimeOffset.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivating_an_inactive_user_keeps_the_first_date_and_author() // USR-30
    {
        var user = TestData.NewUser();
        var firstBy = Guid.CreateVersion7();
        var firstAt = new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero);
        user.Deactivate(firstBy, firstAt);

        var result = user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 21, 9, 0, 0, TimeSpan.Zero));

        result.IsSuccess.ShouldBeTrue();
        user.DeactivatedAt.ShouldBe(firstAt);
        user.DeactivatedBy.ShouldBe(firstBy);
    }

    [Fact]
    public void Nobody_deactivates_themselves() // USR-19, USR-27
    {
        var user = TestData.NewUser();

        var result = user.Deactivate(user.Id, DateTimeOffset.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("self_deactivation");
        result.Error.Message.ShouldBe("Você não pode desativar o seu próprio acesso.");
        user.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void Reactivating_a_user_who_had_a_password_makes_it_active() // USR-18
    {
        var user = TestData.NewUser();
        user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero));

        var result = user.Reactivate();

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public void Reactivating_a_user_who_never_had_a_password_makes_it_pending() // USR-18
    {
        var user = TestData.NewUser();
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero));

        var result = user.Reactivate();

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void Reactivated_user_has_no_deactivation_date_or_author() // users.md data model
    {
        var user = TestData.NewUser();
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero));

        user.Reactivate();

        user.DeactivatedAt.ShouldBeNull();
        user.DeactivatedBy.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)] // pending
    [InlineData(true)] // active
    public void Reactivating_a_user_who_is_not_inactive_is_refused(bool activateFirst) // USR-18, USR-29
    {
        var user = TestData.NewUser();
        if (activateFirst)
        {
            user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));
        }

        var result = user.Reactivate();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("not_inactive");
        result.Error.Message.ShouldBe("Só é possível reativar um acesso desativado.");
    }

    [Fact]
    public void User_may_have_no_profiles() // USR-21, USR-13
    {
        var user = TestData.NewUser();

        user.ProfileIds.ShouldBeEmpty();
    }

    [Fact]
    public void Assigning_profiles_replaces_them_and_ignores_repeated_ids() // ADR-0006
    {
        var user = TestData.NewUser();
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        user.AssignProfiles([first]);

        var result = user.AssignProfiles([second, second]);

        result.IsSuccess.ShouldBeTrue();
        user.ProfileIds.ShouldBe([second]);
    }

    [Fact]
    public void Admin_has_the_fixed_system_values_and_the_configured_email() // USR-23, USR-24, USR-32
    {
        var email = EmailAddress.Create("admin@example.com").Value;

        var admin = User.CreateAdmin(email);

        admin.Id.ShouldBe(SystemIds.AdminUser);
        admin.Login.Value.ShouldBe("admin");
        admin.DisplayName.ShouldBe("Admin");
        admin.FullName.ShouldBe("Administrador do sistema");
        admin.Email.ShouldBe(email);
        admin.Status.ShouldBe(UserStatus.Active);
        admin.IsSystem.ShouldBeTrue();
        admin.ProfileIds.ShouldBe([SystemIds.ManagerProfile]);
        admin.ActivatedAt.ShouldBeNull();
    }

    [Fact]
    public void Admin_cannot_be_deactivated() // USR-19, USR-23, USR-28
    {
        var admin = TestData.NewAdmin();

        var result = admin.Deactivate(Guid.CreateVersion7(), DateTimeOffset.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("system_record");
        result.Error.Message.ShouldBe("O usuário Admin é do sistema e não pode ser alterado nem desativado.");
        admin.Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public void Admin_profiles_cannot_change() // USR-24, USR-28
    {
        var admin = TestData.NewAdmin();

        var result = admin.AssignProfiles([Guid.CreateVersion7()]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("system_record");
        result.Error.Message.ShouldBe("O usuário Admin é do sistema e não pode ser alterado nem desativado.");
        admin.ProfileIds.ShouldBe([SystemIds.ManagerProfile]);
    }
}
