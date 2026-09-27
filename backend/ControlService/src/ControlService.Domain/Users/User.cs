namespace ControlService.Domain.Users;

public sealed class User
{
    private User(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    public UserStatus Status { get; private set; } = UserStatus.Pending;

    public static User Create() => new(Guid.CreateVersion7());
}
