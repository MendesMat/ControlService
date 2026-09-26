using ControlService.Domain.ValueObjects;

namespace ControlService.Domain.Tests.ValueObjects;

public class ScreenKeysTests
{
    [Fact]
    public void ScreenKeys_users_screen_matches_the_catalog() // ADR-0021, docs/product/screen-catalog.json
    {
        ScreenKeys.GerenciamentoUsuarios.ShouldBe("gerenciamento/usuarios");
        ScreenKeys.All.ShouldContain("gerenciamento/usuarios");
    }
}
