using ControlService.Application.Commercial.Customers.Queries;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.SeedWork;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;

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
        var customer = BuildCustomer(Guid.NewGuid(), Document.Create("19103190072"));
        var customerId = customer.Id;
        _repository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Type.Should().Be(customer.Type.ToString());
        result.LegalName.Should().Be(customer.LegalName);
        result.TradeName.Should().Be(customer.TradeName);
        result.Status.Should().Be(customer.Status.ToString());
        result.City.Should().Be(customer.Address.City);
        result.State.Should().Be(customer.Address.State);
        result.Document.Should().Be(customer.Document?.GetFormattedValue());
        result.DocumentType.Should().Be(customer.Document?.Type.ToString());
    }

    [Fact]
    public async Task Handle_CustomerWithoutDocument_ReturnsDtoWithNullDocument()
    {
        // Arrange
        var customer = BuildCustomer(Guid.NewGuid(), null);
        var customerId = customer.Id;
        _repository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Document.Should().BeNull();
        result.DocumentType.Should().BeNull();
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
        (await act.Should().ThrowAsync<EntityNotFoundException>())
            .WithMessage($"Customer com identificador '{customerId}' não foi encontrado.");
    }

    private static Customer BuildCustomer(Guid id, Document? document = null) =>
        new(CustomerType.Business, "Acme Corp", "Acme", document,
            ControlService.Domain.Commercial.Customers.ValueObjects.Address.Create(
                "01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));
}
