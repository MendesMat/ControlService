namespace ControlService.Domain.Access;

public enum AccessLevel
{
    Denied,
    Reader,
    Editor,
    Manager,
}

public static class AccessLevelExtensions
{
    public static string ToWireValue(this AccessLevel level) => level switch
    {
        AccessLevel.Denied => "negado",
        AccessLevel.Reader => "leitor",
        AccessLevel.Editor => "editor",
        AccessLevel.Manager => "gerenciador",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, message: null),
    };

    public static bool TryFromWireValue(string wireValue, out AccessLevel level)
    {
        var known = wireValue switch
        {
            "negado" => AccessLevel.Denied,
            "leitor" => AccessLevel.Reader,
            "editor" => AccessLevel.Editor,
            "gerenciador" => AccessLevel.Manager,
            _ => (AccessLevel?)null,
        };

        level = known.GetValueOrDefault();
        return known.HasValue;
    }
}
