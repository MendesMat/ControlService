using ControlService.API.Auth;

namespace ControlService.API.Common;

/// <summary>The `/api/v1` group every feature registers its endpoints under (ADR-0003, API-01).
/// `Program.cs` and the test factories both map it through here.</summary>
public static class ApiV1Group
{
    /// <summary>Every endpoint requires a signed-in person unless it opts out with `AllowAnonymous()`, and
    /// refuses a person who still has to replace the initial password unless it opts out with
    /// `AllowPendingPasswordChange()` (AUTH-14, ADR-0032).</summary>
    public static RouteGroupBuilder MapApiV1(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/v1").RequireAuthorization().AddEndpointFilter<PasswordChangeRequiredFilter>();
}
