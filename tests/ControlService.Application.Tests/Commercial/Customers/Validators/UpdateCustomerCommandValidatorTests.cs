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
        var command = new UpdateCustomerCommand
        {
            Id = Guid.NewGuid(),
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
}
