using ControlService.Domain.PermissionProfiles;

namespace ControlService.Domain.Access;

/// <summary>Highest access level among a user's permission profiles, screen by screen (PERM-05, PERM-06).</summary>
public static class EffectiveAccess
{
    public static AccessLevel GetLevel(IEnumerable<PermissionProfile> profiles, ScreenKey screen)
    {
        var levels = profiles.Select(profile => profile.GetLevel(screen));
        return levels.DefaultIfEmpty(AccessLevel.Denied).Max();
    }

    /// <summary>Resolves the user's profile ids against the known profiles, ignoring ids that match
    /// no profile (PERM-05), then computes the effective level as above.</summary>
    public static AccessLevel GetLevel(
        IEnumerable<Guid> profileIds, IReadOnlyDictionary<Guid, PermissionProfile> profilesById, ScreenKey screen)
    {
        var profiles = profileIds
            .Select(id => profilesById.GetValueOrDefault(id))
            .OfType<PermissionProfile>();

        return GetLevel(profiles, screen);
    }
}
