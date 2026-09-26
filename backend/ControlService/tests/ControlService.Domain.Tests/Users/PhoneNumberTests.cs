using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class PhoneNumberTests
{
    [Fact]
    public void Phone_with_11_digits_is_valid() // USR-10
    {
        var result = PhoneNumber.Create("21987654321");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("21987654321");
    }

    [Fact]
    public void Phone_with_10_digits_is_valid() // USR-10
    {
        var result = PhoneNumber.Create("2134567890");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("2134567890");
    }

    [Fact]
    public void Phone_with_mask_is_stored_as_digits_only() // CNV-07
    {
        var result = PhoneNumber.Create("(21) 98765-4321");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("21987654321");
    }

    [Fact]
    public void Phone_with_9_digits_is_rejected_with_message() // USR-10
    {
        var result = PhoneNumber.Create("219876543");

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Digite o telefone com DDD. Ex.: (21) 98765-4321.");
    }

    [Fact]
    public void Phone_with_12_digits_is_rejected() // USR-10
    {
        var result = PhoneNumber.Create("219876543210");

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Digite o telefone com DDD. Ex.: (21) 98765-4321.");
    }
}
