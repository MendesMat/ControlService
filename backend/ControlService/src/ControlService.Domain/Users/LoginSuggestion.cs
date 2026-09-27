using System.Globalization;
using System.Text.RegularExpressions;
using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

/// <summary>Suggests a login from a full name, so the person can accept it or type another one (USR-14, USR-15).</summary>
public static partial class LoginSuggestion
{
    private const int MaxLength = 30; // USR-05

    public static string Suggest(string fullName, IReadOnlyCollection<string> takenLogins)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var baseLogin = parts.Length == 1 ? parts[0] : $"{parts[0]}.{parts[^1]}";
        var normalized = TextNormalization.Normalize(baseLogin);
        var cleanedBase = DisallowedCharacters().Replace(normalized, string.Empty);
        var takenLoginsIgnoringCase = takenLogins.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidate = BuildCandidate(cleanedBase, suffix: "");
        for (var suffixNumber = 2; takenLoginsIgnoringCase.Contains(candidate); suffixNumber++)
        {
            candidate = BuildCandidate(cleanedBase, suffixNumber.ToString(CultureInfo.InvariantCulture));
        }

        return candidate;
    }

    private static string BuildCandidate(string baseLogin, string suffix)
    {
        var maxBaseLength = MaxLength - suffix.Length;
        var truncatedBase = baseLogin.Length > maxBaseLength ? baseLogin[..maxBaseLength] : baseLogin;
        return truncatedBase.TrimEnd('.', '-', '_') + suffix;
    }

    [GeneratedRegex("[^a-z0-9._-]")]
    private static partial Regex DisallowedCharacters();
}
