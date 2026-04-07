using ControlService.Application.Commercial.Customers.Queries;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;

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

    [Fact]
    public async Task Handle_WhenCustomersExist_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var document = Document.Create("19103190072");
        var address = Address.Create("01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var customer = new Customer(CustomerType.Business, "Legal Name", "Trade Name", document, address);

        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Customer> { customer });

        var query = new GetAllCustomersQuery();

        // Act
        var resultList = await _handler.Handle(query, CancellationToken.None);
        var result = resultList.First();

        // Assert
        result.Id.Should().Be(customer.Id);
        result.Type.Should().Be(CustomerType.Business.ToString());
        result.LegalName.Should().Be("Legal Name");
        result.TradeName.Should().Be("Trade Name");
        result.Document.Should().Be(document.GetFormattedValue());
        result.DocumentType.Should().Be(DocumentType.CPF.ToString());
        result.Status.Should().Be(CustomerStatus.Active.ToString());
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
    }

    [Fact]
    public async Task Handle_WhenCustomerHasNoDocument_ShouldMapDocumentFieldsAsNull()
    {
        // Arrange
        var customer = BuildCustomer(Guid.NewGuid(), "Client No Doc", null);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Customer> { customer });

        var query = new GetAllCustomersQuery();

        // Act
        var resultList = await _handler.Handle(query, CancellationToken.None);
        var result = resultList.First();

        // Assert
        result.Document.Should().BeNull();
        result.DocumentType.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _repository.GetAllAsync(token).Returns(new List<Customer>());

        var query = new GetAllCustomersQuery();

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _repository.Received(1).GetAllAsync(token);
    }

    private static Customer BuildCustomer(Guid id, string name, Document? document = null) =>
        new(CustomerType.Business, name, name, document,
            Address.Create("01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));
}
