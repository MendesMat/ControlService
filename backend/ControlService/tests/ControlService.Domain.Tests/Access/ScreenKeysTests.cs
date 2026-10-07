using ControlService.Domain.Access;

namespace ControlService.Domain.Tests.Access;

public class ScreenKeysTests
{
    [Fact]
    public void ScreenKeys_users_screen_matches_the_catalog() // docs/product/screen-catalog.json
    {
        ScreenKeys.Users.ShouldBe("gerenciamento/usuarios");
        ScreenKeys.All.ShouldContain("gerenciamento/usuarios");
    }
}
