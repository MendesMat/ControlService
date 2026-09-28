using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Domain.PermissionProfiles;

public sealed class PermissionProfile
{
    private readonly Dictionary<ScreenKey, AccessLevel> _levels = [];

    private PermissionProfile(Guid id, bool isSystem, string name, string description)
    {
        Id = id;
        IsSystem = isSystem;
        Name = name.Trim();
        NormalizedName = TextNormalization.Normalize(Name);
        Description = description.Trim();
    }

    public Guid Id { get; }

    public bool IsSystem { get; }

    public string Name { get; }

    public string NormalizedName { get; }

    public string Description { get; }

    public IReadOnlyDictionary<ScreenKey, AccessLevel> Levels => _levels;

    public static PermissionProfile Create(string name, string description) =>
        new(Guid.CreateVersion7(), isSystem: false, name, description);

    public static PermissionProfile CreateManagerProfile() =>
        new(SystemIds.ManagerProfile, isSystem: true, "Gerenciador", "Acesso total a todas as telas do sistema.");

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

        if (level == AccessLevel.Denied)
        {
            _levels.Remove(screen);
            return Result.Success();
        }

        _levels[screen] = level;
        return Result.Success();
    }

    public Result Delete() => EnsureNotSystem();

    private Result EnsureNotSystem() => IsSystem
        ? Result.Failure(new Error(
            "system_record",
            "O perfil Gerenciador é do sistema e não pode ser alterado nem excluído."))
        : Result.Success();
}
