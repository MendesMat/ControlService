using ControlService.Domain.Access;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles;

/// <summary>One `{ screen, level }` item of a profile as sent: the screen key and the level's wire value, not yet checked.</summary>
public sealed record LevelInput(string Screen, string Level)
{
    /// <summary>Checks every item and fails with the first screen or level the server does not know, whole,
    /// instead of ignoring it: a dropped level would look saved and grant nothing.</summary>
    public static Result<IReadOnlyList<ScreenLevel>> ParseAll(IEnumerable<LevelInput> inputs)
    {
        var parsed = new List<ScreenLevel>();
        foreach (var input in inputs)
        {
            var screen = ScreenKey.Create(input.Screen);
            if (screen.IsFailure)
            {
                return Result<IReadOnlyList<ScreenLevel>>.Failure(PermissionProfileErrors.InvalidLevels(screen.Error.Message));
            }

            if (!AccessLevelExtensions.TryFromWireValue(input.Level, out var level))
            {
                return Result<IReadOnlyList<ScreenLevel>>.Failure(
                    PermissionProfileErrors.InvalidLevels(PermissionProfileErrors.UnknownLevelMessage));
            }

            parsed.Add(new ScreenLevel(screen.Value, level));
        }

        return Result<IReadOnlyList<ScreenLevel>>.Success(parsed);
    }
}
