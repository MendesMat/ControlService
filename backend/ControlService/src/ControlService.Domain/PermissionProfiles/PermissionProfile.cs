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

    public AccessLevel GetLevel(ScreenKey screen) =>
        _levels.TryGetValue(screen, out var level) ? level : AccessLevel.Denied;

    public void SetLevel(ScreenKey screen, AccessLevel level) => _levels[screen] = level;
}
