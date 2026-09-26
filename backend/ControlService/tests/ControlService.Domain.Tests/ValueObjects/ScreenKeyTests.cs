using ControlService.Domain.ValueObjects;

namespace ControlService.Domain.Tests.ValueObjects;

public class ScreenKeyTests
{
    [Fact]
    public void ScreenKey_accepts_a_key_from_the_catalog() // ADR-0021
    {
        var result = ScreenKey.Create("gerenciamento/usuarios");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("gerenciamento/usuarios");
    }

    [Fact]
    public void ScreenKey_rejects_a_key_not_in_the_catalog() // ADR-0021
    {
        var result = ScreenKey.Create("gerenciamento/tela-que-nao-existe");

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe("Unknown screen key.");
    }
}
