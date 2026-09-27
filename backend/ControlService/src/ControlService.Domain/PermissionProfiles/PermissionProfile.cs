using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Domain.PermissionProfiles;

public sealed class PermissionProfile
{
    private readonly Dictionary<ScreenKey, AccessLevel> _levels = [];

    private PermissionProfile(Guid id, bool isSystem)
    {
        Id = id;
        IsSystem = isSystem;
    }

    public Guid Id { get; }

    public bool IsSystem { get; }

    public static PermissionProfile Create() => new(Guid.CreateVersion7(), isSystem: false);

    public static PermissionProfile CreateManagerProfile() => new(SystemIds.ManagerProfile, isSystem: true);

    public AccessLevel GetLevel(ScreenKey screen)
    {
        if (IsSystem)
        {
            return AccessLevel.Manager;
        }

        return _levels.TryGetValue(screen, out var level) ? level : AccessLevel.Denied;
    }

    public Result SetLevel(ScreenKey screen, AccessLevel level)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        _levels[screen] = level;
        return Result.Success();
    }

    /// <summary>Guards the invariant that the Gerenciador profile can never be deleted (PERM-22).
    /// Whether the profile is still in use by a user (PERM-20) is checked by the handler.</summary>
    public Result Delete() => EnsureNotSystem();

    private Result EnsureNotSystem() => IsSystem
        ? Result.Failure(new Error(
            "system_record",
            "O perfil Gerenciador é do sistema e não pode ser alterado nem excluído."))
        : Result.Success();
}
