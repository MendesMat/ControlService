using FluentAssertions;
using System;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("(21) 99999-9999", "21999999999", PhoneType.Mobile)]
    [InlineData("21 9 9999-9999", "21999999999", PhoneType.WhatsApp)]
    [InlineData("(21) 3333-4444", "2133334444", PhoneType.Landline)]
    [InlineData("(21) 3333-4444", "2133334444", PhoneType.Fax)]
    public void Given_ValidFormattedNumber_When_Created_Then_ShouldStripNonDigits(
        string rawInput, string expectedValue, PhoneType type)
    {
        var phone = Phone.Create(rawInput, type);

        phone.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Given_ValidInput_When_CreatedWithIsMain_Then_ShouldSetPropertiesCorrectly()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);

        phone.Value.Should().Be("21999999999");
        phone.Type.Should().Be(PhoneType.Mobile);
        phone.IsMainNumber.Should().BeTrue();
    }

    [Fact]
    public void Given_ValidInputWithoutIsMainParameter_When_Created_Then_IsMainShouldBeFalse()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile);

        phone.IsMainNumber.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_NullOrEmptyNumber_When_Created_Then_ShouldThrowDomainException(string invalidInput)
    {
        Action action = () => Phone.Create(invalidInput, PhoneType.Mobile);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid phone number length.");
    }

    [Theory]
    [InlineData("123456789")]   // 9 digits — too short
    [InlineData("123")]
    public void Given_TooShortNumber_When_Created_Then_ShouldThrowDomainException(string invalidInput)
    {
        Action action = () => Phone.Create(invalidInput, PhoneType.Landline);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid phone number length.");
    }

    [Theory]
    [InlineData("219999999999")]   // 12 digits — too long
    [InlineData("1234567890123")]  // 13 digits — too long
    public void Given_TooLongNumber_When_Created_Then_ShouldThrowDomainException(string invalidInput)
    {
        Action action = () => Phone.Create(invalidInput, PhoneType.Mobile);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid phone number length.");
    }

    [Fact]
    public void Given_ElevenDigitNumber_When_GetFormattedValue_Then_ShouldFormatAsMobile()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile);

        phone.GetFormattedValue().Should().Be("(21) 99999-9999");
    }

    [Fact]
    public void Given_TenDigitNumber_When_GetFormattedValue_Then_ShouldFormatAsLandline()
    {
        var phone = Phone.Create("2133334444", PhoneType.Landline);

        phone.GetFormattedValue().Should().Be("(21) 3333-4444");
    }

    [Fact]
    public void Given_SameValues_When_Compared_Then_ShouldBeEqual()
    {
        var first = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);
        var second = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);

        first.Should().Be(second);
    }

    [Fact]
    public void Given_DifferentValue_When_Compared_Then_ShouldNotBeEqual()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);
        var phoneWithDifferentNumber = Phone.Create("21988887777", PhoneType.Mobile, isMain: true);

        phone.Should().NotBe(phoneWithDifferentNumber);
    }

    [Fact]
    public void Given_DifferentType_When_Compared_Then_ShouldNotBeEqual()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);
        var phoneWithDifferentType = Phone.Create("21999999999", PhoneType.WhatsApp, isMain: true);

        phone.Should().NotBe(phoneWithDifferentType);
    }

    [Fact]
    public void Given_DifferentIsMainNumber_When_Compared_Then_ShouldNotBeEqual()
    {
        var phone = Phone.Create("21999999999", PhoneType.Mobile, isMain: true);
        var phoneWithDifferentIsMain = Phone.Create("21999999999", PhoneType.Mobile, isMain: false);

        phone.Should().NotBe(phoneWithDifferentIsMain);
    }
}
