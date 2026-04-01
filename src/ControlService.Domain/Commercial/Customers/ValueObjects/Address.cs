using System.Text.RegularExpressions;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Address : ValueObject
{
    public string PostalCode { get; }
    public string Street { get; }
    public string? Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }

    private Address(string postalCode, string street, string? number, string? complement, string neighborhood, string city, string state)
    {
        PostalCode = postalCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state.ToUpperInvariant();
    }

    public static Address Create(string postalCode, string street, string? number, string? complement, string neighborhood, string city, string state)
    {
        var rawPostalCode = Regex.Replace(postalCode ?? string.Empty, "[^0-9]", "");

        if (string.IsNullOrWhiteSpace(state) || state.Trim().Length != 2)
            throw new DomainException("State must be a 2-character abbreviation.");

        return new Address(rawPostalCode, street, number, complement, neighborhood, city, state);
    }

    public string GetFormattedPostalCode()
    {
        if (PostalCode.Length == 8)
            return $"{PostalCode.Substring(0, 5)}-{PostalCode.Substring(5, 3)}";
        return PostalCode;
    }

    public string GetFullAddress()
    {
        var complementPart = string.IsNullOrWhiteSpace(Complement) ? string.Empty : $" - {Complement}";
        var numberPart = string.IsNullOrWhiteSpace(Number) ? "S/N" : Number;

        return $"{Street}, {numberPart}{complementPart}, {Neighborhood}. {City}/{State}. CEP: {GetFormattedPostalCode()}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PostalCode;
        yield return Street;
        yield return Number ?? string.Empty;
        yield return Complement ?? string.Empty;
        yield return Neighborhood;
        yield return City;
        yield return State;
    }
}
