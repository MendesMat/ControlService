using ControlService.API.Auth;

namespace ControlService.API.Common;

/// <summary>The `/api/v1` group every feature registers its endpoints under (API-01).
/// `Program.cs` and the test factories both map it through here.</summary>
public static class ApiV1Group
{
    public const string Prefix = "/api/v1";

    /// <summary>Every endpoint requires a signed-in person unless it opts out with `AllowAnonymous()`.
    /// The group filters run in the order they are added, which is the order of the refusals (AUTH-30):
    /// first the account must be active (AUTH-18), then a person who still has to replace the temporary
    /// password is refused unless the endpoint opts out with `AllowPendingPasswordChange()` (AUTH-14).</summary>
    public static RouteGroupBuilder MapApiV1(this IEndpointRouteBuilder app) =>
        app.MapGroup(Prefix)
            .RequireAuthorization()
            .AddEndpointFilter<AccountCheckFilter>()
            .AddEndpointFilter<PasswordChangeRequiredFilter>();
}
