using System.Text.RegularExpressions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class TaxInscription : ValueObject
{
    public string Value { get; }
    public TaxInscriptionType Type { get; }

    protected TaxInscription() { } // Para o EF Core

    private TaxInscription(string value, TaxInscriptionType type)
    {
        Value = value;
        Type = type;
    }

    public static TaxInscription Create(string value, TaxInscriptionType type)
    {
        var rawValue = RemoveFormatting(value);
        return new TaxInscription(rawValue, type);
    }

    private static string RemoveFormatting(string value)
    {
        return Regex.Replace(value ?? string.Empty, "[^0-9]", "");
    }

    public string GetFormattedValue()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}
