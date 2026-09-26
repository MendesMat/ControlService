using ControlService.Domain.ValueObjects;

namespace ControlService.Domain.Tests.ValueObjects;

public class LoginTests
{
    [Fact]
    public void Login_with_valid_characters_is_accepted() // USR-05
    {
        var result = Login.Create("ana.souza");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("ana.souza");
    }

    [Fact]
    public void Login_shorter_than_3_characters_is_rejected() // USR-05
    {
        var result = Login.Create("an");

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe(
            "O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado.");
    }

    [Fact]
    public void Login_longer_than_30_characters_is_rejected() // USR-05
    {
        var result = Login.Create(new string('a', 31));

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe(
            "O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado.");
    }

    [Fact]
    public void Login_is_stored_lowercase() // USR-05
    {
        var result = Login.Create("Ana.Souza");

        result.Value.Value.ShouldBe("ana.souza");
    }

    [Theory]
    [InlineData("ana souza")]
    [InlineData("ana@souza")]
    [InlineData("ana_sôuza")]
    public void Login_with_invalid_character_is_rejected(string login) // USR-05
    {
        var result = Login.Create(login);

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe(
            "O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado.");
    }
}
