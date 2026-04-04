using MediatR;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Queries;

/// <summary>
/// Consulta para listar todos os clientes cadastrados.
/// </summary>
public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerResponseDto>>
{
}
