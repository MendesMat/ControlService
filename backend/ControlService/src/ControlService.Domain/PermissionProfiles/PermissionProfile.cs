using ControlService.Domain.Access;

namespace ControlService.Domain.PermissionProfiles;

public sealed class PermissionProfile
{
    private readonly Dictionary<ScreenKey, AccessLevel> _levels = [];

    private PermissionProfile()
    {
    }

    public static PermissionProfile Create() => new();

    public AccessLevel GetLevel(ScreenKey screen) =>
        _levels.TryGetValue(screen, out var level) ? level : AccessLevel.Denied;

    public void SetLevel(ScreenKey screen, AccessLevel level) => _levels[screen] = level;
}
