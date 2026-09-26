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
}
