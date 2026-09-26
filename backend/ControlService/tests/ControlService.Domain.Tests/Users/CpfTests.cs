using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void Cpf_with_valid_check_digits_is_accepted(string cpf) // USR-08
    {
        var result = Cpf.Create(cpf);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(cpf);
    }

    [Fact]
    public void Cpf_with_mask_is_stored_as_digits_only() // CNV-07
    {
        var result = Cpf.Create("529.982.247-25");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("52998224725");
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    public void Cpf_with_all_equal_digits_is_rejected(string cpf) // USR-08
    {
        var result = Cpf.Create(cpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Este CPF não é válido. Confira os números ou deixe o campo em branco.");
    }

    [Theory]
    [InlineData("52998224726")]
    [InlineData("11144477730")]
    public void Cpf_with_invalid_check_digit_is_rejected(string cpf) // USR-08
    {
        var result = Cpf.Create(cpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Este CPF não é válido. Confira os números ou deixe o campo em branco.");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Cpf_with_wrong_length_is_rejected(string cpf) // USR-08
    {
        var result = Cpf.Create(cpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Este CPF não é válido. Confira os números ou deixe o campo em branco.");
    }
}
