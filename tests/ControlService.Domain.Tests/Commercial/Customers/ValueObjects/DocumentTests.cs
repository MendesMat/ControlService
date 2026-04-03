using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;
using FluentAssertions;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class DocumentTests
{
    [Theory]
    [InlineData("123.456.789-00", DocumentType.CPF, "12345678900")]
    [InlineData("12.345.678/0001-90", DocumentType.CNPJ, "12345678000190")]
    [InlineData("A1B2C3.4D5E6.7F8G9-0H0I", DocumentType.CPF, "12345678900")]
    public void Create_ValidDocument_ShouldReturnDocumentWithRawValue(string formattedValue, DocumentType type, string expectedRawValue)
    {
        // Act
        var document = Document.Create(formattedValue, type);

        // Assert
        document.Value.Should().Be(expectedRawValue);
        document.Type.Should().Be(type);
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
    [InlineData("123456780001901", DocumentType.CNPJ)]
    public void Create_InvalidLength_ShouldThrowDomainException(string invalidValue, DocumentType type)
    {
        // Act
        Action action = () => Document.Create(invalidValue, type);

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage($"Invalid length for {type} document.");
    }

    [Fact]
    public void GetFormattedValue_Cpf_ShouldReturnFormattedString()
    {
        // Arrange
        var document = Document.Create("12345678900", DocumentType.CPF);

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("123.456.789-00");
    }

    [Fact]
    public void GetFormattedValue_Cnpj_ShouldReturnFormattedString()
    {
        // Arrange
        var document = Document.Create("12345678000190", DocumentType.CNPJ);

        // Act
        var formattedValue = document.GetFormattedValue();

        // Assert
        formattedValue.Should().Be("12.345.678/0001-90");
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
        formattedValue.Should().Be("12345");
    }

    [Fact]
    public void IsValid_ShouldReturnTrue()
    {
        // Arrange
        var document = Document.Create("12345678900", DocumentType.CPF);

        // Act
        var isValid = document.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Equals_DocumentsWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var documentFirst = Document.Create("12345678900", DocumentType.CPF);
        var documentSecond = Document.Create("12345678900", DocumentType.CPF);

        // Act / Assert
        documentFirst.Equals(documentSecond).Should().BeTrue();
        (documentFirst == documentSecond).Should().BeTrue();
        (documentFirst != documentSecond).Should().BeFalse();
    }

    [Fact]
    public void Equals_DocumentsWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var documentFirst = Document.Create("12345678900", DocumentType.CPF);
        var documentSecond = Document.Create("09876543211", DocumentType.CPF);
        var documentThird = Document.Create("12345678000190", DocumentType.CNPJ);

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
        var documentFirst = Document.Create("12345678000190", DocumentType.CNPJ);
        var documentSecond = Document.Create("12345678000190", DocumentType.CNPJ);

        // Act
        var firstHashCode = documentFirst.GetHashCode();
        var secondHashCode = documentSecond.GetHashCode();

        // Assert
        firstHashCode.Should().Be(secondHashCode);
    }
}
