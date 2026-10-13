using ControlService.Api.IntegrationTests.Common;
using ControlService.API.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Access;

public sealed class EndpointDeclarationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Any active account may call these, whatever its levels. A new route never joins this list by accident:
    // the owner decides, and the list is the only place to do it (API-11).
    private static readonly string[] SignedInOnly = ["/api/v1/auth/sign-out", "/api/v1/auth/change-password", "/api/v1/me"];

    // #9-T19
    [Fact]
    public void Every_api_v1_route_declares_its_screen_and_level_unless_it_is_anonymous_or_signed_in_only() // API-11
    {
        var undeclared = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api/v1/", StringComparison.Ordinal) == true)
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null
                && endpoint.Metadata.GetMetadata<ScreenAccessMetadata>() is null
                && !SignedInOnly.Contains(endpoint.RoutePattern.RawText))
            .Select(endpoint => endpoint.DisplayName ?? endpoint.RoutePattern.RawText)
            .ToList();

        undeclared.ShouldBeEmpty("These routes declare no screen and level (call RequireScreenAccess): " + string.Join(", ", undeclared));
    }
}
