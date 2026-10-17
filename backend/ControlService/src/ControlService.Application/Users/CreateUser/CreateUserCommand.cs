namespace ControlService.Application.Users.CreateUser;

/// <summary>Everything the registration form sends, not yet checked. A blank optional field is empty text;
/// `ProfileIds` that do not exist are discarded (USR-13). `TemporaryPassword` is never part of the record (AUTH-19).</summary>
public sealed record CreateUserCommand(
    string Login,
    string Email,
    string FullName,
    string DisplayName,
    string Cpf,
    string Rg,
    string BirthDate,
    string BloodType,
    string Phone,
    AddressInput Address,
    EmergencyContactInput EmergencyContact,
    IReadOnlyList<Guid> ProfileIds,
    string TemporaryPassword);

public sealed record AddressInput(string Cep, string Street, string Number, string Complement, string District, string City);

public sealed record EmergencyContactInput(string Name, string Relationship, string Phone);
