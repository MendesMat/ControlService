using System.Globalization;
using System.Security.Claims;
using ControlService.API.Common;
using ControlService.Application.Auth;
using ControlService.Application.Auth.GetMe;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/auth/sign-in", SignIn)
            .AllowAnonymous()
            .WithSummary("Signs in with login and password; sets the refresh cookie.");

        api.MapGet("/me", Me)
            .WithSummary("The signed-in person and their effective level on every screen.");

        return api;
    }

    private static async Task<Results<Ok<MeResponse>, ProblemHttpResult>> Me(
        ClaimsPrincipal principal,
        IQueryHandler<GetMeQuery, MeResponse> handler,
        CancellationToken cancellationToken)
    {
        var userId = principal.UserId();
        if (userId is null)
        {
            return AuthErrors.SessionExpired.ToProblem();
        }

        var result = await handler.Handle(new GetMeQuery(userId.Value), cancellationToken);
        return result.IsFailure ? result.Error.ToProblem() : TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<SessionResponse>, ProblemHttpResult>> SignIn(
        SignInRequest request,
        ICommandHandler<SignInCommand, SessionGrant> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new SignInCommand(request.Login, request.Password), cancellationToken);
        if (result.IsFailure)
        {
            return SignInProblem(result.Error, httpContext);
        }

        RefreshCookie.Append(httpContext.Response, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return TypedResults.Ok(SessionResponse.From(result.Value));
    }

    // The 403 of account_inactive is sign-in only, and the lockout tells the browser how long to wait.
    private static ProblemHttpResult SignInProblem(Error error, HttpContext httpContext)
    {
        if (error.Code == "account_inactive")
        {
            return error.ToProblem(StatusCodes.Status403Forbidden);
        }

        if (error.Details is not null && error.Details.TryGetValue("retryAfterSeconds", out var seconds))
        {
            httpContext.Response.Headers.RetryAfter = Convert.ToString(seconds, CultureInfo.InvariantCulture);
        }

        return error.ToProblem();
    }

    private sealed record SignInRequest(string Login = "", string Password = "");
}
