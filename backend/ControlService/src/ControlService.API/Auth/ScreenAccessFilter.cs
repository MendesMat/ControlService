using ControlService.API.Common;
using ControlService.Application.Auth;
using ControlService.Application.Auth.CheckScreenAccess;
using ControlService.Application.Common;
using ControlService.Domain.Access;

namespace ControlService.API.Auth;

/// <summary>Refuses with 403 `forbidden` when the person's effective level on the screen the endpoint
/// declared is below the minimum it declared (PERM-03, PERM-12). Reads the database on every request,
/// so a change to a profile counts from the next one (PERM-13).</summary>
public sealed class ScreenAccessFilter(IQueryHandler<CheckScreenAccessQuery, Unit> handler) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var requirement = httpContext.GetEndpoint()?.Metadata.GetMetadata<ScreenAccessMetadata>()
            ?? throw new InvalidOperationException("ScreenAccessFilter runs only on endpoints that call RequireScreenAccess.");

        var userId = httpContext.User.UserId();
        if (userId is null)
        {
            return AuthErrors.SessionExpired.ToProblem();
        }

        var query = new CheckScreenAccessQuery(userId.Value, requirement.Screen, requirement.Minimum, requirement.AlternativeScreen);
        var result = await handler.Handle(query, httpContext.RequestAborted);
        return result.IsFailure ? result.Error.ToProblem() : await next(context);
    }
}

/// <summary>What an endpoint declares with <see cref="ScreenAccessExtensions.RequireScreenAccess{TBuilder}"/> (API-11).</summary>
public sealed record ScreenAccessMetadata(ScreenKey Screen, AccessLevel Minimum, ScreenKey? AlternativeScreen);

public static class ScreenAccessExtensions
{
    /// <summary>Declares the screen and the minimum level the operation needs. `alternativeScreen` is for the
    /// exception of PERM-04: the same level on that screen is enough too. An unknown screen key throws when
    /// the route is mapped, so a typo never reaches a request.</summary>
    public static TBuilder RequireScreenAccess<TBuilder>(
        this TBuilder builder, string screen, AccessLevel minimum, string? alternativeScreen = null)
        where TBuilder : IEndpointConventionBuilder
    {
        var alternative = alternativeScreen is null ? null : ToScreenKey(alternativeScreen);

        return builder
            .WithMetadata(new ScreenAccessMetadata(ToScreenKey(screen), minimum, alternative))
            .AddEndpointFilter<TBuilder, ScreenAccessFilter>();
    }

    private static ScreenKey ToScreenKey(string screen)
    {
        var screenKey = ScreenKey.Create(screen);
        return screenKey.IsSuccess
            ? screenKey.Value
            : throw new ArgumentException($"'{screen}' is not a screen of the catalog.", nameof(screen));
    }
}
