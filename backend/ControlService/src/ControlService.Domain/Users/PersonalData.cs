namespace ControlService.Domain.Users;

/// <summary>What the registration keeps about the person besides the names and the access: all of it optional
/// and personal data under the LGPD (blood type is sensitive). A missing value is `null`, or empty text.</summary>
public sealed record PersonalData
{
    public static PersonalData Empty { get; } = new();

    public Cpf? Cpf { get; private init; }

    public string Rg { get; private init; } = string.Empty;

    public DateOnly? BirthDate { get; private init; }

    public BloodType? BloodType { get; private init; }

    public PhoneNumber? Phone { get; private init; }

    public Address Address { get; private init; } = Address.Empty;

    public EmergencyContact EmergencyContact { get; private init; } = EmergencyContact.Empty;

    public static PersonalData Create(
        Cpf? cpf,
        string rg,
        DateOnly? birthDate,
        BloodType? bloodType,
        PhoneNumber? phone,
        Address address,
        EmergencyContact emergencyContact) => new()
        {
            Cpf = cpf,
            Rg = rg.Trim(),
            BirthDate = birthDate,
            BloodType = bloodType,
            Phone = phone,
            Address = address,
            EmergencyContact = emergencyContact,
        };
}
