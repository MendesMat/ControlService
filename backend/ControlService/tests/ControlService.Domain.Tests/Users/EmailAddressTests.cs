using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class EmailAddressTests
{
    [Fact]
    public void EmailAddress_with_valid_format_is_accepted() // USR-07
    {
        var result = EmailAddress.Create("ana@empresa.com.br");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("ana@empresa.com.br");
    }

    // #11-T02
    [Fact]
    public void EmailAddress_left_blank_means_no_email() // USR-07
    {
        var result = EmailAddress.Create("");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("ana")]
    [InlineData("ana@")]
    [InlineData("@empresa.com")]
    public void EmailAddress_with_invalid_format_is_rejected(string email) // USR-07
    {
        var result = EmailAddress.Create(email);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe(
            "Este e-mail não parece válido. Confira se ele tem @ e o domínio, por exemplo ana@empresa.com.br.");
    }
}
