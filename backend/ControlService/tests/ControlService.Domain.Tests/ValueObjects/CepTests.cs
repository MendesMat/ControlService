using ControlService.Domain.ValueObjects;

namespace ControlService.Domain.Tests.ValueObjects;

public class CepTests
{
    [Fact]
    public void Cep_with_8_digits_is_valid() // USR-11
    {
        var result = Cep.Create("20040020");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("20040020");
    }

    [Fact]
    public void Cep_with_mask_is_stored_as_digits_only() // CNV-07
    {
        var result = Cep.Create("20040-020");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("20040020");
    }

    [Fact]
    public void Cep_with_7_digits_is_rejected() // USR-11
    {
        var result = Cep.Create("2004002");

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe("O CEP precisa ter 8 números.");
    }
}
