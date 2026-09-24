using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ControlService.Api.IntegrationTests;

public sealed class ApiStartupTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Api_starts_and_serves_the_openapi_document()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
