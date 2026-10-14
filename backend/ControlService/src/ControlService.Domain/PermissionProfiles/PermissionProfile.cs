using System.Diagnostics.CodeAnalysis;
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
        SetDetails(name, description);
    }

    public Guid Id { get; }

    public bool IsSystem { get; }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    public string Description { get; private set; }

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

    /// <summary>Replaces every level at once: a screen left out becomes Denied, and a screen that appears
    /// twice keeps the last level.</summary>
    public Result ReplaceLevels(IEnumerable<ScreenLevel> levels)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        _levels.Clear();
        foreach (var entry in levels)
        {
            SetLevel(entry.Screen, entry.Level);
        }

        return Result.Success();
    }

    public Result ChangeDetails(string name, string description)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        SetDetails(name, description);
        return Result.Success();
    }

    public Result Delete() => EnsureNotSystem();

    [MemberNotNull(nameof(Name), nameof(NormalizedName), nameof(Description))]
    private void SetDetails(string name, string description)
    {
        Name = name.Trim();
        NormalizedName = TextNormalization.Normalize(Name);
        Description = description.Trim();
    }

    private Result EnsureNotSystem() => IsSystem
        ? Result.Failure(new Error(
            "system_record",
            "O perfil Gerenciador é do sistema e não pode ser alterado nem excluído."))
        : Result.Success();
}
