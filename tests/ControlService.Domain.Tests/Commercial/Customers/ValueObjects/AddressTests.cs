using FluentAssertions;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_ValidInput_ShouldSetProperties()
    {
        var address = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "ny");

        address.PostalCode.Should().Be("12345678");
        address.Street.Should().Be("Main St");
        address.Number.Should().Be("123");
        address.Complement.Should().Be("Apt 2");
        address.Neighborhood.Should().Be("Downtown");
        address.City.Should().Be("Metropolis");
        address.State.Should().Be("NY");
    }

    [Fact]
    public void GetFormattedPostalCode_LengthIs8_ShouldFormatPattern()
    {
        var address = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NY");
        address.GetFormattedPostalCode().Should().Be("12345-678");
    }

    [Fact]
    public void GetFullAddress_ValidInputs_ShouldFormatCompleteString()
    {
        var address = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NY");
        address.GetFullAddress().Should().Be("Main St, 123 - Apt 2, Downtown. Metropolis/NY. CEP: 12345-678");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ABC")]
    [InlineData("New York")]
    public void Create_InvalidState_ShouldThrowException(string? invalidState)
    {
        Action action = () => Address.Create("12345678", "Main St", "123", null, "Downtown", "Metropolis", invalidState!);
        action.Should().Throw<DomainException>().WithMessage("State must be a 2-character abbreviation.");
    }

    [Theory]
    [InlineData("12345-678", "12345678")]
    [InlineData("12.345-678", "12345678")]
    [InlineData("12345678abc", "12345678")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    public void Create_PostalCodeWithNonNumericChars_ShouldCleanPostalCode(string? inputPostalCode, string? expectedPostalCode)
    {
        var address = Address.Create(inputPostalCode, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        address.PostalCode.Should().Be(expectedPostalCode);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("")]
    public void GetFormattedPostalCode_PostalCodeLengthNot8_ShouldReturnRawPostalCode(string postalCode)
    {
        var address = Address.Create(postalCode, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        var result = address.GetFormattedPostalCode();

        result.Should().Be(address.PostalCode);
    }

    [Fact]
    public void GetFormattedPostalCode_NullPostalCode_ShouldReturnNull()
    {
        var address = Address.Create(null, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        address.GetFormattedPostalCode().Should().BeNull();
    }

    [Fact]
    public void GetFullAddress_WithoutComplement_ShouldNotFormatComplement()
    {
        var address = Address.Create("12345678", "Main St", "123", null, "Downtown", "Metropolis", "NY");

        var result = address.GetFullAddress();

        result.Should().Be("Main St, 123, Downtown. Metropolis/NY. CEP: 12345-678");
    }

    [Fact]
    public void GetFullAddress_WithoutNumber_ShouldFormatAsSN()
    {
        var address = Address.Create("12345678", "Main St", null, "Apt 2", "Downtown", "Metropolis", "NY");

        var result = address.GetFullAddress();

        result.Should().Be("Main St, S/N - Apt 2, Downtown. Metropolis/NY. CEP: 12345-678");
    }

    [Fact]
    public void GetFullAddress_WithoutNumberAndComplement_ShouldFormatCorrectly()
    {
        var address = Address.Create("12345678", "Main St", "", "  ", "Downtown", "Metropolis", "NY");

        var result = address.GetFullAddress();

        result.Should().Be("Main St, S/N, Downtown. Metropolis/NY. CEP: 12345-678");
    }

    [Fact]
    public void GetFullAddress_WithNullPostalCode_ShouldOmitPostalCodePart()
    {
        var address = Address.Create(null, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        var result = address.GetFullAddress();

        result.Should().Be("Main St, 123, Downtown. Metropolis/NY");
        result.Should().NotContain("CEP");
    }

    [Fact]
    public void Equals_SameValues_ShouldBeEqual()
    {
        var address1 = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "ny");
        var address2 = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NY");

        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_NullNumberAndComplement_ShouldBeEqualToEmptyStrings()
    {
        var address1 = Address.Create("12345678", "Main St", null, null, "Downtown", "Metropolis", "NY");
        var address2 = Address.Create("12345678", "Main St", "", "", "Downtown", "Metropolis", "NY");

        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_DifferentValues_ShouldNotBeEqual()
    {
        var address1 = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NY");
        var address2 = Address.Create("87654321", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NY");
        var address3 = Address.Create("12345678", "Other St", "123", "Apt 2", "Downtown", "Metropolis", "NY");
        var address4 = Address.Create("12345678", "Main St", "321", "Apt 2", "Downtown", "Metropolis", "NY");
        var address5 = Address.Create("12345678", "Main St", "123", "Apt 3", "Downtown", "Metropolis", "NY");
        var address6 = Address.Create("12345678", "Main St", "123", "Apt 2", "Uptown", "Metropolis", "NY");
        var address7 = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Gotham", "NY");
        var address8 = Address.Create("12345678", "Main St", "123", "Apt 2", "Downtown", "Metropolis", "NJ");

        address1.Should().NotBe(address2);
        address1.Should().NotBe(address3);
        address1.Should().NotBe(address4);
        address1.Should().NotBe(address5);
        address1.Should().NotBe(address6);
        address1.Should().NotBe(address7);
        address1.Should().NotBe(address8);
    }

    [Fact]
    public void Equals_BothWithNullPostalCode_ShouldBeEqual()
    {
        var address1 = Address.Create(null, "Main St", "123", null, "Downtown", "Metropolis", "NY");
        var address2 = Address.Create(null, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_OneWithNullPostalCodeAndOtherWithValue_ShouldNotBeEqual()
    {
        var addressWithPostalCode = Address.Create("12345678", "Main St", "123", null, "Downtown", "Metropolis", "NY");
        var addressWithoutPostalCode = Address.Create(null, "Main St", "123", null, "Downtown", "Metropolis", "NY");

        addressWithPostalCode.Should().NotBe(addressWithoutPostalCode);
    }
}
