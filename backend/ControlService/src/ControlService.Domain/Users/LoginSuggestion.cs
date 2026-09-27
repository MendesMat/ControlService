using System.Text.RegularExpressions;
using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

/// <summary>Suggests a login from a full name, so the person can accept it or type another one (USR-14, USR-15).</summary>
public static partial class LoginSuggestion
{
    public static string Suggest(string fullName, IReadOnlyCollection<string> takenLogins)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var baseLogin = parts.Length == 1 ? parts[0] : $"{parts[0]}.{parts[^1]}";
        var normalized = TextNormalization.Normalize(baseLogin);
        return DisallowedCharacters().Replace(normalized, string.Empty);
    }

    [GeneratedRegex("[^a-z0-9._-]")]
    private static partial Regex DisallowedCharacters();
}
