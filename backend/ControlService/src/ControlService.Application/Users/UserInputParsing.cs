using System.Globalization;
using ControlService.Application.Users.CreateUser;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Users;

/// <summary>Turns the text the person typed into the domain's value objects. It runs after the validator, so a
/// value that is not blank is already known to be valid.</summary>
internal static class UserInputParsing
{
    public const string BirthDateFormat = "yyyy-MM-dd";

    public static bool TryParseBirthDate(string text, out DateOnly date) =>
        DateOnly.TryParseExact(text.Trim(), BirthDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static PersonalData ToPersonalData(this CreateUserCommand command) => PersonalData.Create(
        Optional(command.Cpf, Cpf.Create),
        command.Rg,
        TryParseBirthDate(command.BirthDate, out var birthDate) ? birthDate : null,
        Optional(command.BloodType, BloodType.Create),
        Optional(command.Phone, PhoneNumber.Create),
        Address.Create(
            Optional(command.Address.Cep, Cep.Create),
            command.Address.Street,
            command.Address.Number,
            command.Address.Complement,
            command.Address.District,
            command.Address.City),
        EmergencyContact.Create(
            command.EmergencyContact.Name,
            command.EmergencyContact.Relationship,
            Optional(command.EmergencyContact.Phone, PhoneNumber.Create)));

    private static T? Optional<T>(string text, Func<string, Result<T>> create)
        where T : class =>
        string.IsNullOrWhiteSpace(text) ? null : create(text).Value;
}
