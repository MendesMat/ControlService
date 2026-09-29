using System.Net;
using ControlService.Api.IntegrationTests.Common;

namespace ControlService.Api.IntegrationTests;

public sealed class ApiStartupTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Api_starts_and_serves_the_openapi_document()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
