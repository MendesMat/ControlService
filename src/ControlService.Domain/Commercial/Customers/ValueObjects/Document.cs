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

    public static Document Create(string value, DocumentType type)
    {
        var rawValue = RemoveFormatting(value);

        if (type == DocumentType.CPF && rawValue.Length != 11)
            throw new DomainException($"Invalid length for {type} document.");

        if (type == DocumentType.CNPJ && rawValue.Length != 14)
            throw new DomainException($"Invalid length for {type} document.");

        return new Document(rawValue, type);
    }

    private static string RemoveFormatting(string value)
    {
        return Regex.Replace(value, "[^0-9]", "");
    }

    public string GetFormattedValue()
    {
        if (Type == DocumentType.CPF && Value.Length == 11)
            return $"{Value.Substring(0, 3)}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}";

        if (Type == DocumentType.CNPJ && Value.Length == 14)
            return $"{Value.Substring(0, 2)}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}";

        return Value;
    }

    public static bool IsValid() => true;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}
