using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles.ListProfiles;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.PermissionProfiles;

public static class PermissionProfilesEndpoints
{
    private const string ProfilesRoute = "/permission-profiles";

    public static RouteGroupBuilder MapPermissionProfilesEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet(ProfilesRoute, ListProfiles)
            .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Reader, ScreenKeys.Users)
            .WithSummary("Every permission profile, with how many people have it and how many screens it grants.");

        return api;
    }

    private static async Task<Results<Ok<IReadOnlyList<ProfileListItemResponse>>, ProblemHttpResult>> ListProfiles(
        IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ListProfilesQuery(), cancellationToken);
        return result.IsFailure ? result.Error.ToProblem() : TypedResults.Ok(result.Value);
    }
}
