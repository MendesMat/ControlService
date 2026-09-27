using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

public sealed class User
{
    private readonly List<Guid> _profileIds = [];

    private User(Guid id, bool isSystem, UserStatus status)
    {
        Id = id;
        IsSystem = isSystem;
        Status = status;
    }

    public Guid Id { get; }

    public bool IsSystem { get; }

    public IReadOnlyCollection<Guid> ProfileIds => _profileIds;

    public UserStatus Status { get; private set; }

    public DateTimeOffset? ActivatedAt { get; private set; }

    public DateTimeOffset? DeactivatedAt { get; private set; }

    public Guid? DeactivatedBy { get; private set; }

    public static User Create() => new(Guid.CreateVersion7(), isSystem: false, UserStatus.Pending);

    public static User CreateAdmin()
    {
        var admin = new User(SystemIds.AdminUser, isSystem: true, UserStatus.Active);
        admin.ReplaceProfiles([SystemIds.ManagerProfile]);
        return admin;
    }

    public Result Activate(DateTimeOffset now)
    {
        if (Status != UserStatus.Pending)
        {
            return Result.Failure(new Error(
                "link_invalid",
                "Este link não vale mais. Peça a quem cadastrou você para reenviar o acesso."));
        }

        Status = UserStatus.Active;
        ActivatedAt = now;
        return Result.Success();
    }

    public Result Deactivate(Guid by, DateTimeOffset now)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        if (by == Id)
        {
            return Result.Failure(new Error(
                "self_deactivation",
                "Você não pode desativar o seu próprio acesso."));
        }

        if (Status == UserStatus.Inactive)
        {
            return Result.Success();
        }

        Status = UserStatus.Inactive;
        DeactivatedAt = now;
        DeactivatedBy = by;
        return Result.Success();
    }

    public Result AssignProfiles(IEnumerable<Guid> profileIds)
    {
        var systemCheck = EnsureNotSystem();
        if (systemCheck.IsFailure)
        {
            return systemCheck;
        }

        ReplaceProfiles(profileIds);
        return Result.Success();
    }

    private void ReplaceProfiles(IEnumerable<Guid> profileIds)
    {
        _profileIds.Clear();
        _profileIds.AddRange(profileIds.Distinct());
    }

    private Result EnsureNotSystem() => IsSystem
        ? Result.Failure(new Error(
            "system_record",
            "O usuário Admin é do sistema e não pode ser alterado nem desativado."))
        : Result.Success();

    public Result Reactivate(DateTimeOffset now)
    {
        if (Status != UserStatus.Inactive)
        {
            return Result.Failure(new Error(
                "not_inactive",
                "Só é possível reativar um acesso desativado."));
        }

        Status = ActivatedAt.HasValue ? UserStatus.Active : UserStatus.Pending;
        DeactivatedAt = null;
        DeactivatedBy = null;
        return Result.Success();
    }
}
