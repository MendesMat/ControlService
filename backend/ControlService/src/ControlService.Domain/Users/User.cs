using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

public sealed class User
{
    private User(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    public UserStatus Status { get; private set; } = UserStatus.Pending;

    public DateTimeOffset? ActivatedAt { get; private set; }

    public DateTimeOffset? DeactivatedAt { get; private set; }

    public Guid? DeactivatedBy { get; private set; }

    public static User Create() => new(Guid.CreateVersion7());

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

    public Result Reactivate(DateTimeOffset now)
    {
        Status = ActivatedAt.HasValue ? UserStatus.Active : UserStatus.Pending;
        DeactivatedAt = null;
        DeactivatedBy = null;
        return Result.Success();
    }
}
