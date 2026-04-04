using System.Text.RegularExpressions;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Address : ValueObject
{
    public string? PostalCode { get; }
    public string Street { get; }
    public string? Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }

    protected Address() { } // Para o EF Core

    private Address(string? postalCode, string street, string? number, string? complement, string neighborhood, string city, string state)
    {
        PostalCode = postalCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state.ToUpperInvariant();
    }

    public static Address Create(string? postalCode, string street, string? number, string? complement, string neighborhood, string city, string state)
    {
        var rawPostalCode = postalCode is null ? null : Regex.Replace(postalCode, "[^0-9]", "");
        var normalizedPostalCode = string.IsNullOrEmpty(rawPostalCode) ? null : rawPostalCode;

        if (string.IsNullOrWhiteSpace(state) || state.Trim().Length != 2)
            throw new DomainException("State must be a 2-character abbreviation.");

        return new Address(normalizedPostalCode, street, number, complement, neighborhood, city, state);
    }

    public string? GetFormattedPostalCode() =>
        PostalCode?.Length == 8
            ? $"{PostalCode.Substring(0, 5)}-{PostalCode.Substring(5, 3)}"
            : PostalCode;

    public string GetFullAddress()
    {
        var complementPart = string.IsNullOrWhiteSpace(Complement) ? string.Empty : $" - {Complement}";
        var numberPart = string.IsNullOrWhiteSpace(Number) ? "S/N" : Number;
        var postalCodePart = string.IsNullOrEmpty(PostalCode) ? string.Empty : $". CEP: {GetFormattedPostalCode()}";

        return $"{Street}, {numberPart}{complementPart}, {Neighborhood}. {City}/{State}{postalCodePart}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PostalCode ?? string.Empty;
        yield return Street;
        yield return Number ?? string.Empty;
        yield return Complement ?? string.Empty;
        yield return Neighborhood;
        yield return City;
        yield return State;
    }
}
