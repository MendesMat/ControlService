using ControlService.API.Common;
using ControlService.Application.Auth;

namespace ControlService.API.Auth;

/// <summary>While the account has not replaced its temporary password, its access token carries
/// `must_change_password` and every endpoint except the ones that opted out answers 401
/// `password_change_required` (AUTH-14).</summary>
public sealed class PasswordChangeRequiredFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var isPending = httpContext.User.HasClaim(
            claim => claim.Type == JwtAccessTokenIssuer.MustChangePasswordClaim
                && string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase));
        var mayProceed = httpContext.GetEndpoint()?.Metadata.GetMetadata<AllowPendingPasswordChangeMetadata>() is not null;

        return isPending && !mayProceed
            ? ValueTask.FromResult<object?>(AuthErrors.PasswordChangeRequired.ToProblem())
            : next(context);
    }
}

/// <summary>Marks an endpoint the person may call before replacing the temporary password: `me`, sign-out
/// and change-password.</summary>
public sealed record AllowPendingPasswordChangeMetadata;

public static class PasswordChangeRequiredExtensions
{
    public static TBuilder AllowPendingPasswordChange<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new AllowPendingPasswordChangeMetadata());
}
