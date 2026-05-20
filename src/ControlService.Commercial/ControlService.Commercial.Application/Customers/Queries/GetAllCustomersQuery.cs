using MediatR;
using ControlService.Commercial.Application.Customers.DTOs;

namespace ControlService.Commercial.Application.Customers.Queries;

/// <summary>
/// Consulta para listar todos os clientes cadastrados.
/// </summary>
public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerResponseDto>>
{
}
