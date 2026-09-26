using ControlService.Domain.ValueObjects;

namespace ControlService.Domain.Tests.ValueObjects;

public class BloodTypeTests
{
    [Theory]
    [InlineData("A+")]
    [InlineData("A-")]
    [InlineData("B+")]
    [InlineData("B-")]
    [InlineData("AB+")]
    [InlineData("AB-")]
    [InlineData("O+")]
    [InlineData("O-")]
    public void BloodType_accepts_each_value_in_the_closed_list(string value) // glossary
    {
        var result = BloodType.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact]
    public void BloodType_rejects_value_outside_the_list() // USR-14
    {
        var result = BloodType.Create("X+");

        result.IsFailure.ShouldBeTrue();
        result.Error!.Message.ShouldBe("Este tipo sanguíneo não é válido.");
    }
}
