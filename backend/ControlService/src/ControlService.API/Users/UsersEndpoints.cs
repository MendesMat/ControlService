using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.Users.SuggestLogin;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.Users;

public static class UsersEndpoints
{
    private const string UsersRoute = "/users";

    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost($"{UsersRoute}/login-suggestion", SuggestLogin)
            .RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)
            .WithSummary("Suggests a login from a full name; the name travels in the body, never in the URL.");

        return api;
    }

    private static async Task<Results<Ok<SuggestLoginResponse>, ProblemHttpResult>> SuggestLogin(
        SuggestLoginRequest request,
        IQueryHandler<SuggestLoginQuery, SuggestLoginResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new SuggestLoginQuery(request.FullName ?? string.Empty), cancellationToken);
        return result.IsFailure ? result.Error.ToProblem() : TypedResults.Ok(result.Value);
    }

    // A missing or null field is treated as empty, so it fails validation instead of crashing (API-12).
    private sealed record SuggestLoginRequest(string? FullName = null);
}
