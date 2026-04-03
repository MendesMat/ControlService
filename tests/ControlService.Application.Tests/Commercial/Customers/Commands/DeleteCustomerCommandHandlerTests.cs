using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Application.Tests.Commercial.Customers.Commands;

public class DeleteCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repository = Substitute.For<ICustomerRepository>();
        _repository.UnitOfWork.Returns(_unitOfWork);

        _handler = new DeleteCustomerCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ReturnsTrue()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = new DeleteCustomerCommand(customer.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ThrowsEntityNotFoundException()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _repository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var command = new DeleteCustomerCommand(unknownId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_RemovesAndSavesExactlyOnce()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = new DeleteCustomerCommand(customer.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Remove(Arg.Any<Customer>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    private static Customer BuildCustomer() =>
        new(CustomerType.Individual, "João Silva", null, null,
            ControlService.Domain.Commercial.Customers.ValueObjects.Address.Create(
                "01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));
}
