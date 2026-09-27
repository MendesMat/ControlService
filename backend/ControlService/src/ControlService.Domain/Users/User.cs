namespace ControlService.Domain.Users;

public sealed class User
{
    private User()
    {
    }

    public UserStatus Status { get; private set; } = UserStatus.Pending;

    public static User Create() => new();
}
