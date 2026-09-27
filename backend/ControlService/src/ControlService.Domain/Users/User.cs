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

    public static User Create() => new(Guid.CreateVersion7());

    public Result Activate(DateTimeOffset now)
    {
        Status = UserStatus.Active;
        ActivatedAt = now;
        return Result.Success();
    }
}
