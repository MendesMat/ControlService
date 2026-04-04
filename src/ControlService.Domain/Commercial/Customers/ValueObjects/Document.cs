using System.Text.RegularExpressions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Document : ValueObject
{
    public string Value { get; }
    public DocumentType Type { get; }

    protected Document() { } // Para o EF Core

    private Document(string value, DocumentType type)
    {
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Cria um documento inferindo o tipo (CPF ou CNPJ) automaticamente.
    /// </summary>
    public static Document Create(string value)
    {
        var cleanValue = CleanValue(value);
        var type = InferType(cleanValue);

        return Create(cleanValue, type);
    }

    /// <summary>
    /// Cria um documento validando contra o tipo informado.
    /// </summary>
    public static Document Create(string value, DocumentType type)
    {
        var cleanValue = CleanValue(value);

        if (type == DocumentType.CPF && !CpfCnpjValidator.IsCpf(cleanValue))
            throw new DomainException("CPF inválido.");

        if (type == DocumentType.CNPJ && !CpfCnpjValidator.IsCnpj(cleanValue))
            throw new DomainException("CNPJ inválido.");

        return new Document(cleanValue, type);
    }

    private static string CleanValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Documento não pode ser vazio.");

        return Regex.Replace(value, "[^0-9a-zA-Z]", "").ToUpper();
    }

    private static DocumentType InferType(string cleanValue)
    {
        return cleanValue.Length switch
        {
            11 => DocumentType.CPF,
            14 => DocumentType.CNPJ,
            _ => throw new DomainException("Tamanho de documento inválido (deve ter 11 ou 14 caracteres).")
        };
    }

    public string GetFormattedValue()
    {
        if (Type == DocumentType.CPF && Value.Length == 11)
            return $"{Value.Substring(0, 3)}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}";

        if (Type == DocumentType.CNPJ && Value.Length == 14)
            return $"{Value.Substring(0, 2)}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}";

        return Value;
    }

    public bool IsValid() => true;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}
