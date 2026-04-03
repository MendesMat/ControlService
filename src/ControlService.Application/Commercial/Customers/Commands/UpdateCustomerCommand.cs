using MediatR;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

public class UpdateCustomerCommand : IRequest<CustomerResponseDto>
{
    public Guid Id { get; set; }

    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public string? Activity { get; set; }
    public string? OperationalNote { get; set; }
    public string? FinancialNote { get; set; }
}
