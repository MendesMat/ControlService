using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles;

/// <summary>The messages and errors of the profile operations, verbatim from
/// docs/product/features/permission-profiles.md.</summary>
public static class PermissionProfileErrors
{
    public const string NameRequiredMessage = "Dê um nome ao perfil.";

    public const string UnknownLevelMessage = "Este nível de acesso não existe. Atualize a página e tente de novo.";

    /// <summary>The profile is still assigned (PERM-20). `userNames` is in alphabetical order, and the message
    /// keeps "(m)" and "(s)" literally, since it must read well for one person or several.</summary>
    public static Error InUse(IReadOnlyList<string> userNames) => new(
        "profile_in_use",
        $"{string.Join(", ", userNames)} usa(m) este perfil. Tire o perfil dessa(s) pessoa(s) na tela Usuários e depois volte para excluir.",
        Details: new Dictionary<string, object?> { ["userNames"] = userNames });

    /// <summary>A screen or a level in `levels` that the server does not know (PERM-24, PERM-27), as one message
    /// under the field.</summary>
    public static Error InvalidLevels(string message) => new(
        "validation_failed",
        "Alguns campos precisam ser corrigidos.",
        Fields: new Dictionary<string, string[]> { ["levels"] = [message] });

    /// <summary>Another profile already has the name (PERM-17); it is shown as stored.</summary>
    public static Error NameTaken(string existingName) => new(
        "validation_failed",
        "Alguns campos precisam ser corrigidos.",
        Fields: new Dictionary<string, string[]> { ["name"] = [$"Já existe um perfil chamado {existingName}. Escolha outro nome."] });
}
