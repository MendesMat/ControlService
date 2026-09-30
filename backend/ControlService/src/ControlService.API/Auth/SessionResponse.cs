using ControlService.Application.Auth;

namespace ControlService.API.Auth;

/// <summary>The body of sign-in, refresh and change-password (D1). `ExpiresIn` is in seconds, not a
/// timestamp, so a wrong clock on the person's computer does not break the refresh decision.</summary>
public sealed record SessionResponse(string AccessToken, int ExpiresIn, bool MustChangePassword)
{
    public static SessionResponse From(SessionGrant grant) => new(grant.AccessToken, grant.ExpiresIn, grant.MustChangePassword);
}
