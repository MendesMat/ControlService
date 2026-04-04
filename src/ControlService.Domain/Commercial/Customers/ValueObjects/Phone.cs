using System.Text.RegularExpressions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Phone : ValueObject
{
    public string Value { get; }
    public PhoneType Type { get; }
    public bool IsMainNumber { get; }

    protected Phone() { } // Para o EF Core

    private Phone(string value, PhoneType type, bool isMainNumber)
    {
        Value = value;
        Type = type;
        IsMainNumber = isMainNumber;
    }

    public static Phone Create(string value, PhoneType type, bool isMain = false)
    {
        var rawValue = RemoveFormatting(value);

        if (string.IsNullOrWhiteSpace(rawValue) || rawValue.Length < 10 || rawValue.Length > 11)
            throw new DomainException("Invalid phone number length.");

        return new Phone(rawValue, type, isMain);
    }

    private static string RemoveFormatting(string value)
    {
        return Regex.Replace(value ?? string.Empty, "[^0-9]", "");
    }

    public string GetFormattedValue()
    {
        if (Value.Length == 11)
            return $"({Value.Substring(0, 2)}) {Value.Substring(2, 5)}-{Value.Substring(7, 4)}";
        if (Value.Length == 10)
            return $"({Value.Substring(0, 2)}) {Value.Substring(2, 4)}-{Value.Substring(6, 4)}";

        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
        yield return IsMainNumber;
    }
}
