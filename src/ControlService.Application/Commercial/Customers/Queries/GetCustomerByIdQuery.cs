using MediatR;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Queries;

/// <summary>
/// Consulta para obter os detalhes de um cliente por ID.
/// </summary>
public class GetCustomerByIdQuery : IRequest<CustomerResponseDto>
{
    /// <summary> Identificador único do cliente. </summary>
    public Guid Id { get; set; }

    public GetCustomerByIdQuery(Guid id) => Id = id;
}
