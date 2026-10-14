using System.Net;
using ControlService.Api.IntegrationTests.Auth;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class ScreenCatalogApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #10-T05
    [Fact]
    public async Task A_signed_in_person_without_any_profile_gets_the_catalog() // decision 25
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);

        using var response = await client.GetWithTokenAsync("/api/v1/screens", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var areas = (await response.ReadJsonAsync()).GetProperty("areas").EnumerateArray().ToList();
        areas.Select(area => area.GetProperty("id").GetString()).ShouldBe(["gerenciamento", "comercial", "financeiro", "relatorios"]);
        var firstScreen = areas[0].GetProperty("screens")[0];
        firstScreen.GetProperty("key").GetString().ShouldBe("gerenciamento/usuarios");
        firstScreen.GetProperty("label").GetString().ShouldBe("Usuários");
    }
}
