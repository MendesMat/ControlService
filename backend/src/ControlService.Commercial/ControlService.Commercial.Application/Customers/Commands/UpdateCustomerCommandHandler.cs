using MediatR;
using ControlService.Commercial.Domain.Customers;
using ControlService.Commercial.Domain.Customers.ValueObjects;
using ControlService.Commercial.Application.Customers.DTOs;
using ControlService.SharedKernel.SeedWork;

namespace ControlService.Commercial.Application.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponseDto>
{
    private readonly ICustomerRepository _repository;

    public UpdateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponseDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), request.Id);

        var address = Address.Create(
            request.PostalCode, request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State);

        customer.UpdateAddress(address);
        customer.UpdateActivity(request.Activity);
        customer.UpdateNotes(request.OperationalNote, request.FinancialNote);

        _repository.Update(customer);
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
