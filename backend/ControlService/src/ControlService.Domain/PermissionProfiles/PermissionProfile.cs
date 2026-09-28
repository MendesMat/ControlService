using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Domain.PermissionProfiles;

public sealed class PermissionProfile : AuditedAggregate
{
    private readonly List<ScreenLevel> _levels = [];

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

    public IReadOnlyCollection<ScreenLevel> Levels => _levels;

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

        var existing = _levels.Find(entry => entry.Screen.Equals(screen));
        return existing is null ? AccessLevel.Denied : existing.Level;
    }

    public Result SetLevel(ScreenKey screen, AccessLevel level)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        _levels.RemoveAll(entry => entry.Screen.Equals(screen));
        if (level == AccessLevel.Denied)
        {
            return Result.Success();
        }

        _levels.Add(new ScreenLevel(screen, level));
        return Result.Success();
    }

    public Result Delete() => EnsureNotSystem();

    private Result EnsureNotSystem() => IsSystem
        ? Result.Failure(new Error(
            "system_record",
            "O perfil Gerenciador é do sistema e não pode ser alterado nem excluído."))
        : Result.Success();
}
