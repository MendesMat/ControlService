using ControlService.Application.Commercial.Customers.Queries;
using ControlService.Application.Commercial.Customers.DTOs;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.SeedWork;
using ControlService.Domain.Commercial.Customers.Enums;

namespace ControlService.Application.Tests.Commercial.Customers.Queries;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly ICustomerRepository _repository;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _repository = Substitute.For<ICustomerRepository>();
        _handler = new GetCustomerByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        _repository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
    }

    [Fact]
    public async Task Handle_NonExistentCustomer_ThrowsEntityNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _repository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    private static Customer BuildCustomer(Guid id) =>
        new(CustomerType.Business, "Acme Corp", "Acme", null,
            ControlService.Domain.Commercial.Customers.ValueObjects.Address.Create(
                "01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));
}
