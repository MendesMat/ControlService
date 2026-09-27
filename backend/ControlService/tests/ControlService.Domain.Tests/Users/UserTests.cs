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
}
