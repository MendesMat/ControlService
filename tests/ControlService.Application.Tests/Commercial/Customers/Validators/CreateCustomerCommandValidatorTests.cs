using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Application.Commercial.Customers.Validators;
using ControlService.Domain.Commercial.Customers.Enums;

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLegalName_HasError(string name)
    {
        var command = new CreateCustomerCommand { LegalName = name };
        var result = _validator.Validate(command);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateCustomerCommand.LegalName));
    }
}
