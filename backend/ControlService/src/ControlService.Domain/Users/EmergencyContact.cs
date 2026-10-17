namespace ControlService.Domain.Users;

/// <summary>Who to call in an emergency; every part is optional, so an empty contact is a valid one (CNV-03).</summary>
public sealed record EmergencyContact
{
    public static EmergencyContact Empty { get; } = new();

    public string Name { get; private init; } = string.Empty;

    public string Relationship { get; private init; } = string.Empty;

    public PhoneNumber? Phone { get; private init; }

    public static EmergencyContact Create(string name, string relationship, PhoneNumber? phone) => new()
    {
        Name = name.Trim(),
        Relationship = relationship.Trim(),
        Phone = phone,
    };
}
