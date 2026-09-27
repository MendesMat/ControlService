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

    [Fact]
    public void Activating_a_pending_user_makes_it_active_with_the_activation_date() // AUTH-02
    {
        var user = User.Create();
        var now = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);

        var result = user.Activate(now);

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Active);
        user.ActivatedAt.ShouldBe(now);
    }

    [Fact]
    public void Activating_an_active_user_is_refused_as_invalid_link() // AUTH-23
    {
        var user = User.Create();
        user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));

        var result = user.Activate(new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("link_invalid");
        result.Error.Message.ShouldBe(
            "Este link não vale mais. Peça a quem cadastrou você para reenviar o acesso.");
    }

    [Fact]
    public void Deactivating_an_active_user_records_status_date_and_author() // USR-16, USR-17
    {
        var user = User.Create();
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
        var user = User.Create();

        var result = user.Deactivate(Guid.CreateVersion7(), DateTimeOffset.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivating_an_inactive_user_keeps_the_first_date_and_author() // USR-30
    {
        var user = User.Create();
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
        var user = User.Create();

        var result = user.Deactivate(user.Id, DateTimeOffset.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("self_deactivation");
        result.Error.Message.ShouldBe("Você não pode desativar o seu próprio acesso.");
        user.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void Reactivating_a_user_who_had_a_password_makes_it_active() // USR-18
    {
        var user = User.Create();
        user.Activate(new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero));
        user.Deactivate(Guid.CreateVersion7(), new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero));

        var result = user.Reactivate(new DateTimeOffset(2026, 9, 21, 9, 0, 0, TimeSpan.Zero));

        result.IsSuccess.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Active);
    }
}
