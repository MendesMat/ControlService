using System.Globalization;
using ControlService.Domain.Users;

namespace ControlService.Application.Users;

/// <summary>A whole user record as the API answers it. Optional values that are missing are `""`, never `null`
/// (CNV-03), including the deactivation fields of a user who was never deactivated. The password is not part
/// of it (AUTH-19). `Version` is text, as in the other records.</summary>
public sealed record UserResponse(
    Guid Id,
    string Login,
    string Email,
    string Status,
    bool IsSystem,
    string DeactivatedAt,
    string DeactivatedBy,
    string DeactivatedByName,
    string FullName,
    string DisplayName,
    string Cpf,
    string Rg,
    string BirthDate,
    string BloodType,
    string Phone,
    AddressResponse Address,
    EmergencyContactResponse EmergencyContact,
    IReadOnlyList<Guid> ProfileIds,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    string CreatedByName,
    DateTimeOffset UpdatedAt,
    Guid UpdatedBy,
    string UpdatedByName,
    string Version);

public sealed record AddressResponse(string Cep, string Street, string Number, string Complement, string District, string City);

public sealed record EmergencyContactResponse(string Name, string Relationship, string Phone);

public static class UserResponseMapping
{
    /// <summary>The ids whose display names a response needs (API-03): who created, who last changed and who deactivated.</summary>
    public static IEnumerable<Guid> AuthorIds(this User user) =>
        user.DeactivatedBy is { } deactivatedBy ? [user.CreatedBy, user.UpdatedBy, deactivatedBy] : [user.CreatedBy, user.UpdatedBy];

    public static UserResponse ToResponse(this User user, IReadOnlyDictionary<Guid, string> displayNames)
    {
        var data = user.PersonalData;
        return new UserResponse(
            user.Id,
            user.Login.Value,
            user.Email.Value,
            user.Status == UserStatus.Active ? "active" : "inactive",
            user.IsSystem,
            user.DeactivatedAt?.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", CultureInfo.InvariantCulture) ?? string.Empty,
            user.DeactivatedBy?.ToString() ?? string.Empty,
            user.DeactivatedBy is { } deactivatedBy ? displayNames[deactivatedBy] : string.Empty,
            user.FullName,
            user.DisplayName,
            data.Cpf?.Value ?? string.Empty,
            data.Rg,
            data.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
            data.BloodType?.Value ?? string.Empty,
            data.Phone?.Value ?? string.Empty,
            new AddressResponse(
                data.Address.Cep?.Value ?? string.Empty,
                data.Address.Street,
                data.Address.Number,
                data.Address.Complement,
                data.Address.District,
                data.Address.City),
            new EmergencyContactResponse(
                data.EmergencyContact.Name,
                data.EmergencyContact.Relationship,
                data.EmergencyContact.Phone?.Value ?? string.Empty),
            user.ProfileIds.ToArray(),
            user.CreatedAt,
            user.CreatedBy,
            displayNames[user.CreatedBy],
            user.UpdatedAt,
            user.UpdatedBy,
            displayNames[user.UpdatedBy],
            user.Version.ToString(CultureInfo.InvariantCulture));
    }
}
