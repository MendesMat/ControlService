using MediatR;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

public class CreateCustomerCommand : IRequest<CustomerResponseDto>
{
    public CustomerType Type { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    
    // Document
    public string? DocumentValue { get; set; }
    public DocumentType? DocumentType { get; set; }

    // Address
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
