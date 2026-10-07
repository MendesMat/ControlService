using System.Net;
using ControlService.Api.IntegrationTests.Common;

namespace ControlService.Api.IntegrationTests;

// The /api/v1 group itself is wiring (API-01): no feature registers a route under it yet,
// so there is no meaningful Red here. This just proves the group mounts without the pipeline breaking.
public sealed class RouteGroupTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task A_request_under_api_v1_is_routed_without_the_pipeline_breaking() // API-01
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/not-a-real-route", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
