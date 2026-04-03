using MediatR;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Queries;

public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerResponseDto>>
{
}
