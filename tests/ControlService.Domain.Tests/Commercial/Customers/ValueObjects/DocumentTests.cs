using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;
using FluentAssertions;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class DocumentTests
{
    [Theory]
    [InlineData("19103190072", DocumentType.CPF, "19103190072")]
    [InlineData("12.345.678/0001-95", DocumentType.CNPJ, "12345678000195")]
    [InlineData("1A.2B3.4C5/0001-27", DocumentType.CNPJ, "1A2B34C5000127")] // Novo padrão alfanumérico
    public void Create_WithValidValue_ShouldReturnCleanedDocumentWithType(string value, DocumentType expectedType, string expectedRawValue)
    {
        // Act
        var document = Document.Create(value);

        // Assert
        document.Value.Should().Be(expectedRawValue);
        document.Type.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("12345")]
    [InlineData("123456780001951")]
    public void Create_InvalidLength_ShouldThrowDomainException(string invalidValue)
    {
        // Act
        Action action = () => Document.Create(invalidValue);

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage("*Tamanho de documento inválido*");
    }

    [Theory]
    [InlineData("11111111111")]     // CPF inválido (Dígitos repetidos)
    [InlineData("12345678901")]     // CPF inválido (Dígitos verificadores errados)
    [InlineData("12345678000100")]  // CNPJ inválido
    public void Create_InvalidCheckDigits_ShouldThrowDomainException(string invalidValue)
    {
        // Act
        Action action = () => Document.Create(invalidValue);

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage("*inválido*");
    }

    [Fact]
    public void GetFormattedValue_Cpf_ShouldReturnFormattedString()
    {
        // Arrange
        var document = Document.Create("19103190072");

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("191.031.900-72");
    }

    [Fact]
    public void GetFormattedValue_Cnpj_ShouldReturnFormattedString()
    {
        // Arrange
        var document = Document.Create("12345678000195");

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("12.345.678/0001-95");
    }

    [Fact]
    public void IsValid_ShouldValidateCheckDigits()
    {
        // Arrange
        var validCpf = "19103190072";
        var invalidCpf = "19103190073";

        // Act / Assert
        Document.Create(validCpf).Should().NotBeNull();
        Action action = () => Document.Create(invalidCpf);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_DocumentsWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var documentFirst = Document.Create("19103190072");
        var documentSecond = Document.Create("19103190072");

        // Act / Assert
        documentFirst.Equals(documentSecond).Should().BeTrue();
        (documentFirst == documentSecond).Should().BeTrue();
        (documentFirst != documentSecond).Should().BeFalse();
    }

    [Fact]
    public void Equals_DocumentsWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var documentFirst = Document.Create("19103190072");
        var documentSecond = Document.Create("06412433082");
        var documentThird = Document.Create("12345678000195");

        // Act / Assert
        documentFirst.Equals(documentSecond).Should().BeFalse();
        documentFirst.Equals(documentThird).Should().BeFalse();
        (documentFirst == documentSecond).Should().BeFalse();
        (documentFirst != documentSecond).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_DocumentsWithSameValues_ShouldHaveSameHashCode()
    {
        // Arrange
        var documentFirst = Document.Create("12345678000195");
        var documentSecond = Document.Create("12345678000195");

        // Act
        var firstHashCode = documentFirst.GetHashCode();
        var secondHashCode = documentSecond.GetHashCode();

        // Assert
        firstHashCode.Should().Be(secondHashCode);
    }
}
