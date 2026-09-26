using System.Text.RegularExpressions;
using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed partial class Login
{
    private Login(string value) => Value = value;

    public string Value { get; }

    public static Result<Login> Create(string login)
    {
        if (login.Length is < 3 or > 30 || !AllowedCharacters().IsMatch(login))
        {
            return Result<Login>.Failure(new Error(
                "validation_failed",
                "O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado."));
        }

        return Result<Login>.Success(new Login(TextNormalization.Normalize(login)));
    }

    [GeneratedRegex("^[A-Za-z0-9._-]+$")]
    private static partial Regex AllowedCharacters();
}
