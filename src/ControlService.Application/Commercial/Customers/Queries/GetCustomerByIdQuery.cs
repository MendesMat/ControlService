using MediatR;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<CustomerResponseDto>
{
    public Guid Id { get; set; }

    public GetCustomerByIdQuery(Guid id) => Id = id;
}
