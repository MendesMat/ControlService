using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.PermissionProfiles.GetProfile;
using ControlService.Application.PermissionProfiles.ListProfiles;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.PermissionProfiles;

public static class PermissionProfilesEndpoints
{
    private const string ProfilesRoute = "/permission-profiles";
    private const string ProfileRoute = "/permission-profiles/{id:guid}";

    public static RouteGroupBuilder MapPermissionProfilesEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet(ProfilesRoute, ListProfiles)
            .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Reader, ScreenKeys.Users)
            .WithSummary("Every permission profile, with how many people have it and how many screens it grants.");

        api.MapGet(ProfileRoute, GetProfile)
            .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Reader)
            .WithSummary("One permission profile with its level on every screen; the version is in the body and in the ETag.");

        return api;
    }

    private static async Task<Results<Ok<IReadOnlyList<ProfileListItemResponse>>, ProblemHttpResult>> ListProfiles(
        IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ListProfilesQuery(), cancellationToken);
        return result.IsFailure ? result.Error.ToProblem() : TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<ProfileResponse>, ProblemHttpResult>> GetProfile(
        Guid id,
        IQueryHandler<GetProfileQuery, ProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetProfileQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        httpContext.Response.WriteETag(result.Value.Version);
        return TypedResults.Ok(result.Value);
    }
}
