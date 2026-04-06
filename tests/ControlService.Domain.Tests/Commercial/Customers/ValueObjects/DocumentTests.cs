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
    public void Create_ValidDocumentWithType_ShouldReturnDocumentWithRawValue(string formattedValue, DocumentType type, string expectedRawValue)
    {
        // Act
        var document = Document.Create(formattedValue, type);

        // Assert
        document.Value.Should().Be(expectedRawValue);
        document.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("19103190072", DocumentType.CPF)]
    [InlineData("12.345.678/0001-95", DocumentType.CNPJ)]
    [InlineData("1A.2B3.4C5/0001-27", DocumentType.CNPJ)] // Novo padrão alfanumérico
    public void Create_WithOnlyValue_ShouldInferType(string value, DocumentType expectedType)
    {
        // Act
        var document = Document.Create(value);

        // Assert
        document.Type.Should().Be(expectedType);
    }

    [Fact]
    public void Create_UnknownDocumentType_ShouldCreateWithoutLengthValidation()
    {
        // Arrange
        const string rawValue = "12345";
        const DocumentType unknownType = (DocumentType)999;

        // Act
        var document = Document.Create(rawValue, unknownType);

        // Assert
        document.Value.Should().Be(rawValue);
        document.Type.Should().Be(unknownType);
    }

    [Theory]
    [InlineData("123", DocumentType.CPF)]
    [InlineData("123456789012", DocumentType.CPF)]
    [InlineData("12345", DocumentType.CNPJ)]
    [InlineData("123456780001951", DocumentType.CNPJ)]
    public void Create_InvalidLength_ShouldThrowDomainException(string invalidValue, DocumentType type)
    {
        // Act
        Action action = () => Document.Create(invalidValue, type);

        // Assert
        action.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("11111111111")] // CPF inválido (Dígitos repetidos)
    [InlineData("12345678901")] // CPF inválido (Dígitos verificadores errados)
    [InlineData("12345678000100")] // CNPJ inválido
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
        var document = Document.Create("19103190072", DocumentType.CPF);

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("191.031.900-72");
    }

    [Fact]
    public void GetFormattedValue_Cnpj_ShouldReturnFormattedString()
    {
        // Arrange
        var document = Document.Create("12345678000195", DocumentType.CNPJ);

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("12.345.678/0001-95");
    }

    [Fact]
    public void GetFormattedValue_UnknownDocumentType_ShouldReturnRawValue()
    {
        // Arrange
        const string rawValue = "12345xyz";
        var document = Document.Create(rawValue, (DocumentType)999);

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("12345XYZ");
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
        var documentFirst = Document.Create("19103190072", DocumentType.CPF);
        var documentSecond = Document.Create("19103190072", DocumentType.CPF);

        // Act / Assert
        documentFirst.Equals(documentSecond).Should().BeTrue();
        (documentFirst == documentSecond).Should().BeTrue();
        (documentFirst != documentSecond).Should().BeFalse();
    }

    [Fact]
    public void Equals_DocumentsWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var documentFirst = Document.Create("19103190072", DocumentType.CPF);
        var documentSecond = Document.Create("06412433082", DocumentType.CPF);
        var documentThird = Document.Create("12345678000195", DocumentType.CNPJ);

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
        var documentFirst = Document.Create("12345678000195", DocumentType.CNPJ);
        var documentSecond = Document.Create("12345678000195", DocumentType.CNPJ);

        // Act
        var firstHashCode = documentFirst.GetHashCode();
        var secondHashCode = documentSecond.GetHashCode();

        // Assert
        firstHashCode.Should().Be(secondHashCode);
    }
}
