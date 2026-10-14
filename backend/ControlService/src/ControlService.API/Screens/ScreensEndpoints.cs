using ControlService.Domain.Access;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.Screens;

public static class ScreensEndpoints
{
    public static RouteGroupBuilder MapScreensEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/screens", ListScreens)
            .WithSummary("The areas and screens of the menu, with the keys that permissions use.");

        return api;
    }

    // The catalog is a fixed list written in code, so there is no handler between the route and the answer.
    private static Ok<ScreenCatalogResponse> ListScreens() => TypedResults.Ok(new ScreenCatalogResponse(ScreenCatalog.Areas));

    private sealed record ScreenCatalogResponse(IReadOnlyList<ScreenArea> Areas);
}
