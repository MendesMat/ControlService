namespace ControlService.Infrastructure.Auth;

/// <summary>One signed-in browser: only the SHA-256 of its refresh token is stored (AUTH-19). An
/// infrastructure row, not an aggregate, so it has no audit fields.</summary>
public sealed class UserSession
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public required byte[] TokenHash { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; set; }
}
