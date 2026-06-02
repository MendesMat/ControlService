using MediatR;
using ControlService.Commercial.Domain.Customers;
using ControlService.Commercial.Domain.Customers.ValueObjects;
using ControlService.Commercial.Domain.Customers.Services;
using ControlService.Commercial.Application.Customers.DTOs;

namespace ControlService.Commercial.Application.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponseDto>
{
    private readonly ICustomerRepository _repository;
    private readonly CustomerDocumentUniquenessService _documentUniquenessService;

    public CreateCustomerCommandHandler(
        ICustomerRepository repository,
        CustomerDocumentUniquenessService documentUniquenessService)
    {
        _repository = repository;
        _documentUniquenessService = documentUniquenessService;
    }

    public async Task<CustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
            request.PostalCode, request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State);

        Document? document = !string.IsNullOrWhiteSpace(request.DocumentValue)
            ? Document.Create(request.DocumentValue)
            : null;

        if (document is not null)
            await _documentUniquenessService.EnforceUniquenessAsync(document, cancellationToken: cancellationToken);

        var customer = new Customer(
            request.Type,
            request.LegalName,
            request.TradeName,
            document,
            address);

        await _repository.AddAsync(customer, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return MapToResponseDto(customer);
    }

    private static CustomerResponseDto MapToResponseDto(Customer customer) => new()
    {
        Id = customer.Id,
        Type = customer.Type.ToString(),
        LegalName = customer.LegalName,
        TradeName = customer.TradeName,
        Document = customer.Document?.GetFormattedValue(),
        DocumentType = customer.Document?.Type.ToString(),
        Status = customer.Status.ToString(),
        City = customer.Address.City,
        State = customer.Address.State
    };
}

