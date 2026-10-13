using ControlService.API.Common;
using ControlService.Application.Auth;
using ControlService.Application.Auth.CheckAccount;
using ControlService.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ControlService.API.Auth;

/// <summary>On every authenticated request, confirms in the database that the account still exists and is
/// active, even with a valid access token: 401 `session_expired` or `account_inactive` (AUTH-18, AUTH-27).
/// Anonymous endpoints are skipped, since they carry no session.</summary>
public sealed class AccountCheckFilter(IQueryHandler<CheckAccountQuery, Unit> handler) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (httpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return await next(context);
        }

        var userId = httpContext.User.UserId();
        if (userId is null)
        {
            return AuthErrors.SessionExpired.ToProblem();
        }

        var result = await handler.Handle(new CheckAccountQuery(userId.Value), httpContext.RequestAborted);
        return result.IsFailure ? result.Error.ToProblem() : await next(context);
    }
}
