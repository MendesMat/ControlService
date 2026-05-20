using MediatR;
using ControlService.Commercial.Application.Customers.DTOs;
using ControlService.Commercial.Domain.Customers;
using ControlService.SharedKernel.SeedWork;

namespace ControlService.Commercial.Application.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponseDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponseDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), request.Id);

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
