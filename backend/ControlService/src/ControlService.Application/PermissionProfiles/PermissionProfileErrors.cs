using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles;

/// <summary>The messages and errors of the profile operations, verbatim from
/// docs/product/features/permission-profiles.md.</summary>
public static class PermissionProfileErrors
{
    public const string NameRequiredMessage = "Dê um nome ao perfil.";

    /// <summary>Another profile already has the name (PERM-17); it is shown as stored.</summary>
    public static Error NameTaken(string existingName) => new(
        "validation_failed",
        "Alguns campos precisam ser corrigidos.",
        Fields: new Dictionary<string, string[]> { ["name"] = [$"Já existe um perfil chamado {existingName}. Escolha outro nome."] });
}
