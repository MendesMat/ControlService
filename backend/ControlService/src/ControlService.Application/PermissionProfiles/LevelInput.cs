using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles;

/// <summary>One `{ screen, level }` item of a profile as sent: the screen key and the level's wire value, not yet checked.</summary>
public sealed record LevelInput(string Screen, string Level)
{
    public static IReadOnlyList<ScreenLevel> ParseAll(IEnumerable<LevelInput> inputs) => inputs
        .Select(input => new ScreenLevel(ScreenKey.Create(input.Screen).Value, AccessLevelExtensions.FromWireValue(input.Level)))
        .ToArray();
}
