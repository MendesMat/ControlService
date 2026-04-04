using MediatR;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

/// <summary>
/// Comando para atualizar os dados de um cliente.
/// </summary>
public class UpdateCustomerCommand : IRequest<CustomerResponseDto>
{
    /// <summary> Identificador do cliente a ser atualizado. </summary>
    public Guid Id { get; set; }

    /// <summary> CEP do endereço. </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary> Nome da rua/logradouro. </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary> Número do endereço. </summary>
    public string? Number { get; set; }

    /// <summary> Complemento. </summary>
    public string? Complement { get; set; }

    /// <summary> Bairro. </summary>
    public string Neighborhood { get; set; } = string.Empty;

    /// <summary> Cidade. </summary>
    public string City { get; set; } = string.Empty;

    /// <summary> Estado. </summary>
    public string State { get; set; } = string.Empty;

    /// <summary> Atividade principal do cliente. </summary>
    public string? Activity { get; set; }

    /// <summary> Observação operacional. </summary>
    public string? OperationalNote { get; set; }

    /// <summary> Observação financeira. </summary>
    public string? FinancialNote { get; set; }
}
