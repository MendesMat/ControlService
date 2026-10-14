using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class CreateProfileApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #10-T15
    [Fact]
    public async Task An_editor_creates_a_profile_with_the_levels_sent() // API-05, CNV-04, CNV-19
    {
        var name = $"Vendas {ProfileTestSupport.UniqueSuffix()}";
        using var client = factory.CreateHttpsClient();
        var (editor, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var body = new
        {
            name,
            description = "Equipe comercial",
            levels = new[]
            {
                new { screen = ScreenKeys.Customers, level = "editor" },
                new { screen = ScreenKeys.AccountsReceivable, level = "leitor" },
            },
        };

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, ProfileTestSupport.ProfilesPath, token, body);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var profile = await response.ReadJsonAsync();
        var id = profile.GetProperty("id").GetGuid();
        id.ShouldNotBe(Guid.Empty);
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"{ProfileTestSupport.ProfilesPath}/{id}");
        var version = profile.GetProperty("version").GetString();
        response.Headers.ETag.ShouldNotBeNull().Tag.ShouldBe($"\"{version}\"");
        profile.GetProperty("name").GetString().ShouldBe(name);
        profile.GetProperty("description").GetString().ShouldBe("Equipe comercial");
        profile.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
        var levels = profile.GetProperty("levels").EnumerateArray().ToList();
        levels.Single(entry => entry.GetProperty("screen").GetString() == ScreenKeys.Customers).GetProperty("level").GetString().ShouldBe("editor");
        levels.Single(entry => entry.GetProperty("screen").GetString() == ScreenKeys.AccountsReceivable).GetProperty("level").GetString().ShouldBe("leitor");
        profile.GetProperty("createdBy").GetGuid().ShouldBe(editor.Id);
        profile.GetProperty("createdByName").GetString().ShouldBe(editor.DisplayName);
        profile.GetProperty("updatedBy").GetGuid().ShouldBe(editor.Id);
        profile.GetProperty("updatedByName").GetString().ShouldBe(editor.DisplayName);
    }

    // #10-T16
    [Fact]
    public async Task A_profile_created_without_levels_has_every_screen_denied() // PERM-18
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var body = new { name = $"Vazio {ProfileTestSupport.UniqueSuffix()}", description = string.Empty };

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, ProfileTestSupport.ProfilesPath, token, body);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var levels = (await response.ReadJsonAsync()).GetProperty("levels").EnumerateArray().ToList();
        levels.Count.ShouldBe(20);
        levels.ShouldAllBe(entry => entry.GetProperty("level").GetString() == "negado");
    }

    // #10-T17
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task A_missing_blank_or_absent_name_is_refused(string? name) // PERM-16, API-15
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        object body = name is null ? new { description = "Sem nome" } : new { name, description = "Sem nome" };

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, ProfileTestSupport.ProfilesPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("name").ShouldBe(["Dê um nome ao perfil."]);
    }

    // #10-T18
    [Fact]
    public async Task A_name_equal_to_another_profiles_ignoring_case_accents_and_spaces_is_refused() // PERM-17, API-13
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Vendedór {suffix}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, ProfileTestSupport.ProfilesPath, token, new { name = $"  VENDEDOR {suffix} ", description = string.Empty });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("name").ShouldBe([$"Já existe um perfil chamado Vendedór {suffix}. Escolha outro nome."]);
    }

    // #10-T19
    [Fact]
    public async Task The_name_gerenciador_is_refused() // PERM-17
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, ProfileTestSupport.ProfilesPath, token, new { name = "gerenciador", description = string.Empty });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("name").ShouldBe(["Já existe um perfil chamado Gerenciador. Escolha outro nome."]);
    }
}
