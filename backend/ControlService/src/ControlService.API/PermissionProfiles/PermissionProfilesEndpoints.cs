using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.PermissionProfiles.CreateProfile;
using ControlService.Application.PermissionProfiles.GetProfile;
using ControlService.Application.PermissionProfiles.ListProfiles;
using ControlService.Application.PermissionProfiles.UpdateProfile;
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

        api.MapPost(ProfilesRoute, CreateProfile)
            .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Editor)
            .WithSummary("Creates a permission profile; a screen left out of `levels` starts as `negado`.");

        api.MapPut(ProfileRoute, UpdateProfile)
            .RequireScreenAccess(ScreenKeys.PermissionProfiles, AccessLevel.Editor)
            .WithSummary("Replaces a profile's name, description and levels; the version read goes in If-Match.");

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

    private static async Task<Results<Created<ProfileResponse>, ProblemHttpResult>> CreateProfile(
        ProfileRequest request,
        ICommandHandler<CreateProfileCommand, ProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateProfileCommand(request.Name ?? string.Empty, request.Description ?? string.Empty, request.ToLevelInputs());
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        httpContext.Response.WriteETag(result.Value.Version);
        return TypedResults.Created($"{ApiV1Group.Prefix}{ProfilesRoute}/{result.Value.Id}", result.Value);
    }

    // Missing or null fields are treated as empty, so they fail validation instead of crashing (API-12).
    private sealed record ProfileRequest(string? Name = null, string? Description = null, IReadOnlyList<ProfileLevelRequest?>? Levels = null)
    {
        public LevelInput[] ToLevelInputs() =>
            (Levels ?? []).Select(level => new LevelInput(level?.Screen ?? string.Empty, level?.Level ?? string.Empty)).ToArray();
    }

    private sealed record ProfileLevelRequest(string? Screen = null, string? Level = null);

    private static async Task<Results<Ok<ProfileResponse>, ProblemHttpResult>> UpdateProfile(
        Guid id,
        ProfileRequest request,
        ICommandHandler<UpdateProfileCommand, ProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand(
            id, httpContext.Request.ReadIfMatch(), request.Name ?? string.Empty, request.Description ?? string.Empty, request.ToLevelInputs());
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        httpContext.Response.WriteETag(result.Value.Version);
        return TypedResults.Ok(result.Value);
    }
}
