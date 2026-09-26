using ControlService.Domain.Common;

namespace ControlService.Domain.Access;

public sealed class ScreenKey
{
    private ScreenKey(string value) => Value = value;

    public string Value { get; }

    public static Result<ScreenKey> Create(string key)
    {
        if (!ScreenKeys.All.Contains(key))
        {
            return Result<ScreenKey>.Failure(new Error(
                "validation_failed",
                "Esta tela não existe mais no sistema. Atualize a página e tente de novo."));
        }

        return Result<ScreenKey>.Success(new ScreenKey(key));
    }
}
