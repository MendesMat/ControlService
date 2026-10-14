using System.Globalization;
using ControlService.Application.Auth.GetMe;
using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles;

/// <summary>A whole profile. `Levels` always has every screen of the catalog, in catalog order, with `negado`
/// for the ones not stored (the format of `me`); `Version` is text, as in the other records.</summary>
public sealed record ProfileResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    IReadOnlyList<ScreenLevelResponse> Levels,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    string CreatedByName,
    DateTimeOffset UpdatedAt,
    Guid UpdatedBy,
    string UpdatedByName,
    string Version);

public static class ProfileResponseMapping
{
    public static ProfileResponse ToResponse(this PermissionProfile profile, IReadOnlyDictionary<Guid, string> displayNames) => new(
        profile.Id,
        profile.Name,
        profile.Description,
        profile.IsSystem,
        ScreenKeys.All
            .Select(key => new ScreenLevelResponse(key, profile.GetLevel(ScreenKey.Create(key).Value).ToWireValue()))
            .ToArray(),
        profile.CreatedAt,
        profile.CreatedBy,
        displayNames[profile.CreatedBy],
        profile.UpdatedAt,
        profile.UpdatedBy,
        displayNames[profile.UpdatedBy],
        profile.Version.ToString(CultureInfo.InvariantCulture));
}
