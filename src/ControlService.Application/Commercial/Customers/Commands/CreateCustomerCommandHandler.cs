using MediatR;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponseDto>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
            request.PostalCode, request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State);

        Document? document = !string.IsNullOrWhiteSpace(request.DocumentValue) 
            ? Document.Create(request.DocumentValue) 
            : null;

        var customer = new Customer(
            request.Type,
            request.LegalName,
            request.TradeName,
            document,
            address);

        await _repository.AddAsync(customer, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new CustomerResponseDto
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
}
