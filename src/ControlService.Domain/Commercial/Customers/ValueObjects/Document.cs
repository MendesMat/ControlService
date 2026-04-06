using System.Text.RegularExpressions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Document : ValueObject
{
    private const int CpfLength = 11;
    private const int CnpjLength = 14;
    private static readonly Regex OnlyAlphanumericRegex = new(@"[^0-9a-zA-Z]", RegexOptions.Compiled);

    public string Value { get; }
    public DocumentType Type { get; }

    protected Document()
    {
        Value = null!;
    } // Para o EF Core

    private Document(string value, DocumentType type)
    {
        Value = value;
        Type = type;
    }

    public static Document Create(string value)
    {
        var cleanValue = CleanValue(value);
        var type = InferType(cleanValue);

        return CreateWithCleanValue(cleanValue, type);
    }

    private static Document CreateWithCleanValue(string cleanValue, DocumentType type)
    {
        Validate(cleanValue, type);
        return new Document(cleanValue, type);
    }

    private static void Validate(string cleanValue, DocumentType type)
    {
        if (type == DocumentType.CPF && !CpfCnpjValidator.IsCpf(cleanValue))
            throw new DomainException("CPF inválido.");

        if (type == DocumentType.CNPJ && !CpfCnpjValidator.IsCnpj(cleanValue))
            throw new DomainException("CNPJ inválido.");
    }

    private static string CleanValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Documento não pode ser vazio.");

        return OnlyAlphanumericRegex.Replace(value, "").ToUpper();
    }

    private static DocumentType InferType(string cleanValue)
    {
        return cleanValue.Length switch
        {
            CpfLength => DocumentType.CPF,
            CnpjLength => DocumentType.CNPJ,
            _ => throw new DomainException($"Tamanho de documento inválido (deve ter {CpfLength} ou {CnpjLength} caracteres).")
        };
    }

    public string GetFormattedValue()
    {
        return Type switch
        {
            DocumentType.CPF when Value.Length == CpfLength => FormatAsCpf(),
            DocumentType.CNPJ when Value.Length == CnpjLength => FormatAsCnpj(),
            _ => Value
        };
    }

    private string FormatAsCpf() =>
        $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    private string FormatAsCnpj() =>
        $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}
