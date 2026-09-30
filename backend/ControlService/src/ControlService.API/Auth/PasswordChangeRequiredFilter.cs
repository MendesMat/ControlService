using ControlService.API.Common;
using ControlService.Application.Auth;

namespace ControlService.API.Auth;

/// <summary>While the Admin has not replaced the initial password, its access token carries
/// `must_change_password` and every endpoint except the ones that opted out answers 401
/// `password_change_required` (AUTH-14, ADR-0032, T8).</summary>
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

/// <summary>Marks an endpoint the person may call before replacing the initial password: `me`, sign-out
/// and change-password (ADR-0032).</summary>
public sealed record AllowPendingPasswordChangeMetadata;

public static class PasswordChangeRequiredExtensions
{
    public static TBuilder AllowPendingPasswordChange<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new AllowPendingPasswordChangeMetadata());
}
