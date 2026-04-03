using MediatR;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.SeedWork;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponseDto>
{
    private readonly ICustomerRepository _repository;

    public UpdateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponseDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), request.Id);

        var address = Address.Create(
            request.PostalCode, request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State);

        customer.UpdateAddress(address);
        customer.UpdateActivity(request.Activity);
        customer.UpdateNotes(request.OperationalNote, request.FinancialNote);

        _repository.Update(customer);
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
