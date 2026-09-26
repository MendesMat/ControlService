using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed class ScreenKey
{
    private ScreenKey(string value) => Value = value;

    public string Value { get; }

    public static Result<ScreenKey> Create(string key)
    {
        if (!ScreenKeys.All.Contains(key))
        {
            return Result<ScreenKey>.Failure(new Error("invalid_screen_key", "Unknown screen key."));
        }

        return Result<ScreenKey>.Success(new ScreenKey(key));
    }
}
