using ControlService.Domain.PermissionProfiles;

namespace ControlService.Domain.Access;

/// <summary>Highest access level among a user's permission profiles, screen by screen (PERM-05, PERM-06).</summary>
public static class EffectiveAccess
{
    public static AccessLevel GetLevel(IEnumerable<PermissionProfile> profiles, ScreenKey screen) =>
        AccessLevel.Denied;
}
