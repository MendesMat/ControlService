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
        Status = UserStatus.Inactive;
        DeactivatedAt = now;
        DeactivatedBy = by;
        return Result.Success();
    }
}
