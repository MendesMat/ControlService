using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Application.Tests.Commercial.Customers.Commands;

public class UpdateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repository = Substitute.For<ICustomerRepository>();
        _repository.UnitOfWork.Returns(_unitOfWork);

        _handler = new UpdateCustomerCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ReturnsUpdatedCustomer()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id, city: "Campinas", state: "SP");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(customer.Id);
        result.City.Should().Be("Campinas");
        result.State.Should().Be("SP");
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ReturnsMappedDtoWithDocument()
    {
        // Arrange
        var document = ControlService.Domain.Commercial.Customers.ValueObjects.Document.Create("19103190072", DocumentType.CPF);
        var customer = BuildCustomer(document: document);
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Document.Should().Be("191.031.900-72");
        result.DocumentType.Should().Be(DocumentType.CPF.ToString());
        result.LegalName.Should().Be(customer.LegalName);
        result.TradeName.Should().Be(customer.TradeName);
        result.Type.Should().Be(customer.Type.ToString());
        result.Status.Should().Be(customer.Status.ToString());
    }

    [Fact]
    public async Task Handle_WithExistingCustomerWithoutDocument_ReturnsMappedDtoWithNullDocument()
    {
        // Arrange
        var customer = BuildCustomer(document: null);
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Document.Should().BeNull();
        result.DocumentType.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidState_ThrowsDomainException()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var command = BuildCommandFor(customer.Id);
        command.State = "INVALID";

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("State must be a 2-character abbreviation.");
    }

    [Fact]
    public async Task Handle_WithOptionalNotes_UpdatesCustomerCorrectly()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id);
        command.Activity = "Retail";
        command.OperationalNote = "Good payer";
        command.FinancialNote = "Credit limit 5000";

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        customer.Activity.Should().Be("Retail");
        customer.OperationalNote.Should().Be("Good payer");
        customer.FinancialNote.Should().Be("Credit limit 5000");
    }

    [Fact]
    public async Task Handle_WithNullOptionalFields_ClearsExistingValues()
    {
        // Arrange
        var customer = BuildCustomer();
        customer.UpdateActivity("Existing Activity");
        customer.UpdateNotes("Old Op Note", "Old Fin Note");

        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id);
        command.Activity = null;
        command.OperationalNote = null;
        command.FinancialNote = null;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        customer.Activity.Should().BeNull();
        customer.OperationalNote.Should().BeNull();
        customer.FinancialNote.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ThrowsEntityNotFoundException()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _repository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var command = BuildCommandFor(unknownId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_PersistsChangesExactlyOnce()
    {
        // Arrange
        var customer = BuildCustomer();
        _repository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommandFor(customer.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Update(Arg.Any<Customer>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    private static Customer BuildCustomer(ControlService.Domain.Commercial.Customers.ValueObjects.Document? document = null) =>
        new(CustomerType.Business, "Acme Corp", "Acme", document,
            ControlService.Domain.Commercial.Customers.ValueObjects.Address.Create(
                "01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"));

    private static UpdateCustomerCommand BuildCommandFor(Guid id, string city = "São Paulo", string state = "SP") => new()
    {
        Id = id,
        PostalCode = "01310-100",
        Street = "Av. Paulista",
        Number = "1000",
        Complement = null,
        Neighborhood = "Bela Vista",
        City = city,
        State = state,
        Activity = null,
        OperationalNote = null,
        FinancialNote = null
    };
}
