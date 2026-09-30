using System.ComponentModel.DataAnnotations;

namespace ControlService.Infrastructure.Auth;

/// <summary>The `Auth` values Infrastructure needs (AUTH-22). The JWT keys of the same section are
/// read by the API, which is where tokens are issued.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Range(1, 24 * 30)]
    public int RefreshTokenIdleHours { get; init; }

    [Range(1, 1000)]
    public int PasswordMinLength { get; init; }

    [Range(1, 1000)]
    public int LockoutMaxFailedAttempts { get; init; }

    [Range(1, 24 * 60)]
    public int LockoutMinutes { get; init; }
}
