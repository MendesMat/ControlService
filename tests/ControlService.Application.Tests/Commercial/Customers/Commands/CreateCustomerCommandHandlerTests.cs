using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Application.Tests.Commercial.Customers.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repository = Substitute.For<ICustomerRepository>();
        _repository.UnitOfWork.Returns(_unitOfWork);

        _handler = new CreateCustomerCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCustomerWithCorrectData()
    {
        // Arrange
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LegalName.Should().Be("Acme Corp");
        result.Type.Should().Be(CustomerType.Business.ToString());
        result.Status.Should().Be(CustomerStatus.Active.ToString());
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
    }

    [Fact]
    public async Task Handle_WithoutDocument_DoesNotThrow()
    {
        // Arrange
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildValidCommand();
        command.DocumentValue = null;
        command.DocumentType = null;

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithValidCommand_PersistsCustomerExactlyOnce()
    {
        // Arrange
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    private static CreateCustomerCommand BuildValidCommand() => new()
    {
        Type = CustomerType.Business,
        LegalName = "Acme Corp",
        TradeName = "Acme",
        PostalCode = "01310-100",
        Street = "Av. Paulista",
        Number = "1000",
        Complement = null,
        Neighborhood = "Bela Vista",
        City = "São Paulo",
        State = "SP"
    };
}
