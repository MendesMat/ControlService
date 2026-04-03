using ControlService.Application.Commercial.Customers.Queries;
using ControlService.Application.Commercial.Customers.DTOs;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;

namespace ControlService.Application.Tests.Commercial.Customers.Queries;

public class GetAllCustomersQueryHandlerTests
{
    private readonly ICustomerRepository _repository;
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomersQueryHandlerTests()
    {
        _repository = Substitute.For<ICustomerRepository>();
        _handler = new GetAllCustomersQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            BuildCustomer(Guid.NewGuid(), "Client A"),
            BuildCustomer(Guid.NewGuid(), "Client B")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(customers);

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.LegalName).Should().Contain(new[] { "Client A", "Client B" });
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Customer>());

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    private static Customer BuildCustomer(Guid id, string name) =>
        new(CustomerType.Business, name, name, null,
            ControlService.Domain.Commercial.Customers.ValueObjects.Address.Create(
                "01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));
}
