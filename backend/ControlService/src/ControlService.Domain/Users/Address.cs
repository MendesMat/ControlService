namespace ControlService.Domain.Users;

/// <summary>The address of a user; every part is optional, so an empty address is a valid one (CNV-03).
/// The state (UF) is not part of it.</summary>
public sealed record Address
{
    public static Address Empty { get; } = new();

    public Cep? Cep { get; private init; }

    public string Street { get; private init; } = string.Empty;

    public string Number { get; private init; } = string.Empty;

    public string Complement { get; private init; } = string.Empty;

    public string District { get; private init; } = string.Empty;

    public string City { get; private init; } = string.Empty;

    public static Address Create(Cep? cep, string street, string number, string complement, string district, string city) => new()
    {
        Cep = cep,
        Street = street.Trim(),
        Number = number.Trim(),
        Complement = complement.Trim(),
        District = district.Trim(),
        City = city.Trim(),
    };
}
