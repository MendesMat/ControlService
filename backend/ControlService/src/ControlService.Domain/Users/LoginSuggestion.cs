using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

/// <summary>Suggests a login from a full name, so the person can accept it or type another one (USR-14, USR-15).</summary>
public static class LoginSuggestion
{
    public static string Suggest(string fullName, IReadOnlyCollection<string> takenLogins)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return TextNormalization.Normalize($"{parts[0]}.{parts[^1]}");
    }
}
