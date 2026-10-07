using System.Security.Claims;

namespace ControlService.API.Auth;

/// <summary>Reads the claims the access token carries.</summary>
internal static class AuthClaims
{
    public static Guid? UserId(this ClaimsPrincipal principal) => GuidClaim(principal, "sub");

    public static Guid? SessionId(this ClaimsPrincipal principal) => GuidClaim(principal, JwtAccessTokenIssuer.SessionIdClaim);

    private static Guid? GuidClaim(ClaimsPrincipal principal, string type) =>
        Guid.TryParse(principal.FindFirstValue(type), out var value) ? value : null;
}
