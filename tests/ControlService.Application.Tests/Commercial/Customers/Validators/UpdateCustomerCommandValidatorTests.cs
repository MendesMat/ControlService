using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Application.Commercial.Customers.Validators;

namespace ControlService.Application.Tests.Commercial.Customers.Validators;

public class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _validator;

    public UpdateCustomerCommandValidatorTests()
    {
        _validator = new UpdateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Identificador do cliente é obrigatório.")]
    public void Validate_InvalidId_ShouldHaveError(string guidString, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Id = Guid.Parse(guidString);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData("12345678901", "CEP deve ter no máximo 10 caracteres.")]
    public void Validate_InvalidPostalCode_ShouldHaveError(string postalCode, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.PostalCode = postalCode;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_NullOrEmptyPostalCode_ShouldNotHaveError(string? postalCode)
    {
        // Arrange
        var command = CreateValidCommand();
        command.PostalCode = postalCode;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCustomerCommand.PostalCode));
    }

    [Theory]
    [InlineData("", "Logradouro é obrigatório.")]
    public void Validate_InvalidStreet_ShouldHaveError(string street, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Street = street;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Street) && e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void Validate_StreetExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Street = new string('A', 201);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Logradouro deve ter no máximo 200 caracteres.");
    }

    [Theory]
    [InlineData("", "Bairro é obrigatório.")]
    public void Validate_InvalidNeighborhood_ShouldHaveError(string neighborhood, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Neighborhood = neighborhood;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void Validate_NeighborhoodExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Neighborhood = new string('A', 101);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Bairro deve ter no máximo 100 caracteres.");
    }

    [Theory]
    [InlineData("", "Cidade é obrigatória.")]
    public void Validate_InvalidCity_ShouldHaveError(string city, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.City = city;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void Validate_CityExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.City = new string('A', 101);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Cidade deve ter no máximo 100 caracteres.");
    }

    [Theory]
    [InlineData("", "Estado é obrigatório.")]
    [InlineData("S", "Estado deve ser a sigla com 2 caracteres (ex: SP).")]
    [InlineData("SPA", "Estado deve ser a sigla com 2 caracteres (ex: SP).")]
    public void Validate_InvalidState_ShouldHaveError(string state, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.State = state;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData(nameof(UpdateCustomerCommand.OperationalNote), "Nota operacional deve ter no máximo 500 caracteres.")]
    [InlineData(nameof(UpdateCustomerCommand.FinancialNote), "Nota financeira deve ter no máximo 500 caracteres.")]
    public void Validate_NotesExceedMaxLength_ShouldHaveError(string propertyName, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand();
        var longText = new string('A', 501);
        
        if (propertyName == nameof(UpdateCustomerCommand.OperationalNote))
            command.OperationalNote = longText;
        else
            command.FinancialNote = longText;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void Validate_ActivityExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Activity = new string('A', 151);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Atividade deve ter no máximo 150 caracteres.");
    }

    private static UpdateCustomerCommand CreateValidCommand()
    {
        return new UpdateCustomerCommand
        {
            Id = Guid.NewGuid(),
            PostalCode = "01310-100",
            Street = "Av Paulista",
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            State = "SP"
        };
    }
}
