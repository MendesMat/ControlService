using MediatR;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.Application.Commercial.Customers.Commands;

/// <summary>
/// Comando para criar um novo cliente.
/// </summary>
public class CreateCustomerCommand : IRequest<CustomerResponseDto>
{
    /// <summary> Tipo do cliente (Pessoa Física ou Pessoa Jurídica). </summary>
    public CustomerType Type { get; set; }

    /// <summary> Nome legal ou razão social do cliente. </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary> Nome fantasia (opcional). </summary>
    public string? TradeName { get; set; }
    
    /// <summary> Valor do número do documento (CPF/CNPJ). </summary>
    public string? DocumentValue { get; set; }

    /// <summary> Tipo do documento informado. </summary>
    public DocumentType? DocumentType { get; set; }

    /// <summary> CEP do endereço. </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary> Nome da rua/logradouro. </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary> Número do endereço. </summary>
    public string? Number { get; set; }

    /// <summary> Complemento do endereço. </summary>
    public string? Complement { get; set; }

    /// <summary> Bairro. </summary>
    public string Neighborhood { get; set; } = string.Empty;

    /// <summary> Cidade. </summary>
    public string City { get; set; } = string.Empty;

    /// <summary> Estado (UF). </summary>
    public string State { get; set; } = string.Empty;
}
