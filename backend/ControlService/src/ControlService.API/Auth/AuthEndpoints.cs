using System.Globalization;
using System.Security.Claims;
using ControlService.API.Common;
using ControlService.Application.Auth;
using ControlService.Application.Auth.ChangePassword;
using ControlService.Application.Auth.GetMe;
using ControlService.Application.Auth.RefreshSession;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Auth.SignOut;
using ControlService.Application.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;

namespace ControlService.API.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/auth/sign-in", SignIn)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.SignInPolicy)
            .WithSummary("Signs in with login and password; sets the refresh cookie.");

        api.MapPost("/auth/refresh", Refresh)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.RefreshPolicy)
            .WithSummary("Renews the session from the refresh cookie and rotates it.");

        api.MapPost("/auth/sign-out", SignOut)
            .AllowPendingPasswordChange()
            .WithSummary("Ends the session named by the access token and expires the refresh cookie.");

        api.MapPost("/auth/change-password", ChangePassword)
            .AllowPendingPasswordChange()
            .WithSummary("Replaces the initial password; ends every session and starts a new one.");

        api.MapGet("/me", Me)
            .AllowPendingPasswordChange()
            .WithSummary("The signed-in person and their effective level on every screen.");

        return api;
    }

    private static async Task<Results<Ok<SessionResponse>, ProblemHttpResult>> Refresh(
        ICommandHandler<RefreshSessionCommand, SessionGrant> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshCookie.Name] ?? string.Empty;
        var result = await handler.Handle(new RefreshSessionCommand(refreshToken), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        RefreshCookie.Append(httpContext.Response, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return TypedResults.Ok(SessionResponse.From(result.Value));
    }

    private static async Task<Results<Ok<SessionResponse>, ProblemHttpResult>> ChangePassword(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        ICommandHandler<ChangePasswordCommand, SessionGrant> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = principal.UserId();
        if (userId is null)
        {
            return AuthErrors.SessionExpired.ToProblem();
        }

        var result = await handler.Handle(
            new ChangePasswordCommand(
                userId.Value, request.Password ?? string.Empty, request.PasswordConfirmation ?? string.Empty), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        RefreshCookie.Append(httpContext.Response, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return TypedResults.Ok(SessionResponse.From(result.Value));
    }

    // The refresh cookie is scoped to the refresh route, so sign-out never receives it: the session comes
    // from the access token's `sid` claim instead (D3).
    private static async Task<Results<NoContent, ProblemHttpResult>> SignOut(
        ClaimsPrincipal principal,
        ICommandHandler<SignOutCommand, Unit> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sessionId = principal.SessionId();
        if (sessionId is null)
        {
            return AuthErrors.SessionExpired.ToProblem();
        }

        await handler.Handle(new SignOutCommand(sessionId.Value), cancellationToken);
        RefreshCookie.Expire(httpContext.Response);
        return TypedResults.NoContent();
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
        var result = await handler.Handle(new SignInCommand(request.Login ?? string.Empty, request.Password ?? string.Empty), cancellationToken);
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

    // A missing or null field is treated as empty, so it fails validation instead of crashing (API-12).
    private sealed record ChangePasswordRequest(string? Password = null, string? PasswordConfirmation = null);

    private sealed record SignInRequest(string? Login = null, string? Password = null);
}
