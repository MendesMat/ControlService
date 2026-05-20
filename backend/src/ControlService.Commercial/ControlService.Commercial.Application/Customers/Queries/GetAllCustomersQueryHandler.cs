using MediatR;
using ControlService.Commercial.Application.Customers.DTOs;
using ControlService.Commercial.Domain.Customers;

namespace ControlService.Commercial.Application.Customers.Queries;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerResponseDto>>
{
    private readonly ICustomerRepository _repository;

    public GetAllCustomersQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CustomerResponseDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _repository.GetAllAsync(cancellationToken);

        return customers.Select(customer => new CustomerResponseDto
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
        }).ToList();
    }
}
