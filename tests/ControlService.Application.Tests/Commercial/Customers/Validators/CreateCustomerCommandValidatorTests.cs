using System.Linq;
using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Application.Commercial.Customers.Validators;
using ControlService.Domain.Commercial.Customers.Enums;
using FluentAssertions;
using Xunit;
using DocumentTypeEnum = ControlService.Domain.Commercial.Customers.Enums.DocumentType;

namespace ControlService.Application.Tests.Commercial.Customers.Validators;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            LegalName = "Valid Corp",
            Type = CustomerType.Business,
            PostalCode = "01310-100",
            Street = "Av Paulista",
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            State = "SP"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CompleteValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Type = CustomerType.Business,
            LegalName = "Valid Corporation",
            TradeName = "Valid Trade",
            DocumentValue = "12.345.678/0001-90",
            DocumentType = DocumentTypeEnum.CNPJ,
            PostalCode = "01310-200",
            Street = "Av. Paulista",
            Number = "1000",
            Complement = "Andar 10",
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            State = "SP"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLegalName_HasError(string? name)
    {
        var command = new CreateCustomerCommand { LegalName = name! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.LegalName));
    }

    [Fact]
    public void Validate_LegalNameTooLong_HasError()
    {
        var command = new CreateCustomerCommand { LegalName = new string('a', 201) };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.LegalName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPostalCode_HasError(string? postalCode)
    {
        var command = new CreateCustomerCommand { PostalCode = postalCode! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.PostalCode));
    }

    [Fact]
    public void Validate_PostalCodeTooLong_HasError()
    {
        var command = new CreateCustomerCommand { PostalCode = "12345678901" };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.PostalCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyStreet_HasError(string? street)
    {
        var command = new CreateCustomerCommand { Street = street! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.Street));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyNeighborhood_HasError(string? neighborhood)
    {
        var command = new CreateCustomerCommand { Neighborhood = neighborhood! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.Neighborhood));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCity_HasError(string? city)
    {
        var command = new CreateCustomerCommand { City = city! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.City));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyState_HasError(string? state)
    {
        var command = new CreateCustomerCommand { State = state! };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.State));
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPA")]
    public void Validate_InvalidStateLength_HasError(string state)
    {
        var command = new CreateCustomerCommand { State = state };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.State));
    }

    [Fact]
    public void Validate_DocumentValueProvidedWithoutType_HasError()
    {
        var command = new CreateCustomerCommand { DocumentValue = "123", DocumentType = null };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.DocumentType));
    }

    [Fact]
    public void Validate_DocumentTypeProvidedWithoutValue_HasError()
    {
        var command = new CreateCustomerCommand { DocumentType = DocumentTypeEnum.CPF, DocumentValue = null };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.DocumentValue));
    }

    [Theory]
    [InlineData(DocumentTypeEnum.CPF, "1234567890")] // 10 decimal digits
    [InlineData(DocumentTypeEnum.CPF, "123456789012")] // 12 decimal digits
    [InlineData(DocumentTypeEnum.CNPJ, "1234567890123")] // 13 decimal digits
    [InlineData(DocumentTypeEnum.CNPJ, "123456789012345")] // 15 decimal digits
    public void Validate_InvalidDocumentLength_HasError(DocumentTypeEnum type, string value)
    {
        var command = new CreateCustomerCommand { DocumentType = type, DocumentValue = value };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.DocumentValue));
    }

    [Theory]
    [InlineData(DocumentTypeEnum.CPF, "123.456.789-01")] // 11 decimal digits with formatting
    [InlineData(DocumentTypeEnum.CNPJ, "12.345.678/0001-90")] // 14 decimal digits with formatting
    public void Validate_ValidDocumentLengthWithFormatting_HasNoErrors(DocumentTypeEnum type, string value)
    {


        // Fix up the command to be otherwise valid
        var command = new CreateCustomerCommand 
        { 
            DocumentType = type, 
            DocumentValue = value,
            LegalName = "Valid",
            PostalCode = "12345",
            Street = "Valid",
            Neighborhood = "Valid",
            City = "Valid",
            State = "SP"
        };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}


