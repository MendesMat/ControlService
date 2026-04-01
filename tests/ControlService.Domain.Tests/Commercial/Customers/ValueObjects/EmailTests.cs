using FluentAssertions;
using System;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Given_ValidEmail_When_Created_Then_ShouldNormalizeValue()
    {
        var rawEmail = " USER@Example.com ";
        var expectedEmail = "user@example.com";

        var email = Email.Create(rawEmail, EmailType.Personal);

        email.Value.Should().Be(expectedEmail);
    }

    [Fact]
    public void Given_ValidEmail_When_Created_Then_ShouldSetPropertiesCorrectly()
    {
        var validEmail = "user@example.com";
        var expectedType = EmailType.Work;
        var expectedIsMain = true;

        var email = Email.Create(validEmail, expectedType, expectedIsMain);

        email.Value.Should().Be(validEmail);
        email.Type.Should().Be(expectedType);
        email.IsMainEmail.Should().BeTrue();
    }

    [Fact]
    public void Given_ValidEmailWithoutIsMainParameter_When_Created_Then_IsMainShouldBeFalse()
    {
        var validEmail = "user@example.com";

        var email = Email.Create(validEmail, EmailType.Personal);

        email.IsMainEmail.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Given_EmptyOrNullEmail_When_Created_Then_ShouldThrowDomainException(string invalidEmail)
    {
        Action action = () => Email.Create(invalidEmail, EmailType.Work);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@.com")]
    [InlineData("@example.com")]
    [InlineData("user@example")] // missing dot
    [InlineData("user@example.")] // missing chars after dot
    [InlineData("user space@example.com")] // contains space
    public void Given_InvalidFormatEmail_When_Created_Then_ShouldThrowDomainException(string invalidEmail)
    {
        Action action = () => Email.Create(invalidEmail, EmailType.Work);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid email format.");
    }

    [Fact]
    public void Given_SameValues_When_Compared_Then_ShouldBeEqual()
    {
        var firstEmail = Email.Create("user@example.com", EmailType.Personal, true);
        var secondEmail = Email.Create("user@example.com", EmailType.Personal, true);

        firstEmail.Should().Be(secondEmail);
    }

    [Fact]
    public void Given_DifferentValues_When_Compared_Then_ShouldNotBeEqual()
    {
        var email = Email.Create("user@example.com", EmailType.Personal, true);
        var emailWithDifferentValue = Email.Create("other@example.com", EmailType.Personal, true);

        email.Should().NotBe(emailWithDifferentValue);
    }

    [Fact]
    public void Given_DifferentType_When_Compared_Then_ShouldNotBeEqual()
    {
        var email = Email.Create("user@example.com", EmailType.Personal, true);
        var emailWithDifferentType = Email.Create("user@example.com", EmailType.Work, true);

        email.Should().NotBe(emailWithDifferentType);
    }

    [Fact]
    public void Given_DifferentIsMainEmail_When_Compared_Then_ShouldNotBeEqual()
    {
        var email = Email.Create("user@example.com", EmailType.Personal, true);
        var emailWithDifferentIsMain = Email.Create("user@example.com", EmailType.Personal, false);

        email.Should().NotBe(emailWithDifferentIsMain);
    }
}
