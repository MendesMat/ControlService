namespace ControlService.Application.Commercial.Customers.DTOs;

/// <summary>
/// Representa os dados básicos de um cliente para retorno da API.
/// </summary>
public class CustomerResponseDto
{
    /// <summary> Identificador único do cliente. </summary>
    public Guid Id { get; set; }

    /// <summary> Tipo do cliente (Pessoa Física ou Jurídica). </summary>
    public string Type { get; set; }

    /// <summary> Nome legal ou razão social. </summary>
    public string LegalName { get; set; }

    /// <summary> Nome fantasia (se aplicável). </summary>
    public string? TradeName { get; set; }

    /// <summary> Número do documento (CPF/CNPJ). </summary>
    public string? Document { get; set; }

    /// <summary> Tipo do documento informado. </summary>
    public string? DocumentType { get; set; }

    /// <summary> Status atual do cliente no sistema. </summary>
    public string? Status { get; set; }

    /// <summary> Cidade do endereço principal. </summary>
    public string? City { get; set; }

    /// <summary> Estado (UF) do endereço principal. </summary>
    public string? State { get; set; }
}
