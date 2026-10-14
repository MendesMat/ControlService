using System.Net;
using System.Text.Json;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class UpdateProfileApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string NotFound = "Registro não encontrado.";

    // #10-T23
    [Fact]
    public async Task An_editor_saves_name_description_and_levels_with_the_current_version() // CNV-10, CNV-12
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Antigo {suffix}", (ScreenKeys.Users, AccessLevel.Reader));
        using var client = factory.CreateHttpsClient();
        var (editor, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (before, etag) = await ReadProfileAsync(client, token, profileId);
        var body = new
        {
            name = $"Financeiro {suffix}",
            description = "Equipe financeira",
            levels = new[] { new { screen = ScreenKeys.AccountsPayable, level = "editor" } },
        };

        using var response = await client.SendWithTokenAsync(HttpMethod.Put, ProfilePath(profileId), token, body, etag);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var saved = await response.ReadJsonAsync();
        var version = saved.GetProperty("version").GetString();
        version.ShouldNotBe(before.GetProperty("version").GetString());
        response.Headers.ETag.ShouldNotBeNull().Tag.ShouldBe($"\"{version}\"");
        saved.GetProperty("name").GetString().ShouldBe($"Financeiro {suffix}");
        saved.GetProperty("description").GetString().ShouldBe("Equipe financeira");
        LevelOf(saved, ScreenKeys.AccountsPayable).ShouldBe("editor");
        LevelOf(saved, ScreenKeys.Users).ShouldBe("negado");
        saved.GetProperty("updatedBy").GetGuid().ShouldBe(editor.Id);
        saved.GetProperty("updatedByName").GetString().ShouldBe(editor.DisplayName);
        saved.GetProperty("createdByName").GetString().ShouldBe("Admin");

        var (reread, _) = await ReadProfileAsync(client, token, profileId);
        reread.GetRawText().ShouldBe(saved.GetRawText());
    }

    // #10-T24
    [Fact]
    public async Task A_profile_can_be_saved_keeping_its_own_name() // PERM-17
    {
        var name = $"Mesmo nome {ProfileTestSupport.UniqueSuffix()}";
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, name);
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (_, etag) = await ReadProfileAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), token, new { name, description = "Nova descrição" }, etag);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #10-T25
    [Fact]
    public async Task Renaming_to_the_name_of_another_profile_is_refused() // PERM-17
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Compras {suffix}");
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Estoque {suffix}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (_, etag) = await ReadProfileAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), token, new { name = $"COMPRAS {suffix}", description = string.Empty }, etag);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("name").ShouldBe([$"Já existe um perfil chamado Compras {suffix}. Escolha outro nome."]);
    }

    // #10-T26
    [Fact]
    public async Task The_body_is_validated_on_a_change_too() // PERM-16
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Para validar {ProfileTestSupport.UniqueSuffix()}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (_, etag) = await ReadProfileAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), token, new { name = "   ", description = string.Empty }, etag);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("name").ShouldBe(["Dê um nome ao perfil."]);
    }

    // #10-T27
    [Fact]
    public async Task Saving_with_an_old_version_is_refused_and_writes_nothing() // CNV-13
    {
        var name = $"Concorrência {ProfileTestSupport.UniqueSuffix()}";
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, name, (ScreenKeys.Customers, AccessLevel.Editor));
        using var client = factory.CreateHttpsClient();
        var (_, tokenA) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (editorB, tokenB) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (_, oldVersion) = await ReadProfileAsync(client, tokenA, profileId);
        // The same content the profile already has: no field changes, so only the version check can refuse A.
        var unchanged = new
        {
            name,
            description = "Perfil de teste",
            levels = new[] { new { screen = ScreenKeys.Customers, level = "editor" } },
        };
        using var savedByB = await client.SendWithTokenAsync(HttpMethod.Put, ProfilePath(profileId), tokenB, unchanged, oldVersion);
        savedByB.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var response = await client.SendWithTokenAsync(HttpMethod.Put, ProfilePath(profileId), tokenA, unchanged, oldVersion);

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "concurrency_conflict", ChangedBy(editorB.DisplayName));
        problem.GetProperty("details").GetProperty("updatedByName").GetString().ShouldBe(editorB.DisplayName);
        var (current, _) = await ReadProfileAsync(client, tokenA, profileId);
        current.GetProperty("updatedByName").GetString().ShouldBe(editorB.DisplayName);
        current.GetProperty("version").GetString().ShouldBe((await savedByB.ReadJsonAsync()).GetProperty("version").GetString());
    }

    // #10-T28
    [Fact]
    public async Task Saving_without_if_match_is_refused_as_a_conflict() // API-06
    {
        using var client = factory.CreateHttpsClient();
        var (creator, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        using var created = await client.SendWithTokenAsync(
            HttpMethod.Post, ProfileTestSupport.ProfilesPath, token,
            new { name = $"Sem If-Match {ProfileTestSupport.UniqueSuffix()}", description = string.Empty });
        var profileId = (await created.ReadJsonAsync()).GetProperty("id").GetGuid();

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), token, new { name = $"Outro {ProfileTestSupport.UniqueSuffix()}", description = "Mudou" });

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "concurrency_conflict", ChangedBy(creator.DisplayName));
        problem.GetProperty("details").GetProperty("updatedByName").GetString().ShouldBe(creator.DisplayName);
    }

    // #10-T29
    [Fact]
    public async Task The_manager_profile_cannot_be_changed() // PERM-22, PERM-25
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var (_, etag) = await ReadProfileAsync(client, token, SystemIds.ManagerProfile);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(SystemIds.ManagerProfile), token,
            new { name = $"Outro nome {ProfileTestSupport.UniqueSuffix()}", description = string.Empty }, etag);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "system_record", SystemRecordMessage);
    }

    // #10-T30
    [Fact]
    public async Task Changing_an_unknown_profile_answers_not_found() // API-16
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(Guid.NewGuid()), token, new { name = "Qualquer", description = string.Empty }, "\"1\"");

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "not_found", NotFound);
    }

    // #10-T31
    [Fact]
    public async Task A_reader_on_permissions_cannot_change_a_profile() // PERM-03
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Somente leitura {ProfileTestSupport.UniqueSuffix()}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);
        var (_, etag) = await ReadProfileAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), token, new { name = "Mudou", description = string.Empty }, etag);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }

    internal const string SystemRecordMessage = "O perfil Gerenciador é do sistema e não pode ser alterado nem excluído.";

    internal static string ChangedBy(string displayName) =>
        $"Este cadastro foi alterado por {displayName} enquanto você editava. Recarregue para ver a versão atual.";

    private static string ProfilePath(Guid id) => $"{ProfileTestSupport.ProfilesPath}/{id}";

    private static string LevelOf(JsonElement profile, string screen) => profile.GetProperty("levels").EnumerateArray()
        .Single(entry => entry.GetProperty("screen").GetString() == screen).GetProperty("level").GetString()!;

    /// <summary>Reads the profile and the `ETag` exactly as the screen would send it back in `If-Match`.</summary>
    private static async Task<(JsonElement Profile, string ETag)> ReadProfileAsync(HttpClient client, string token, Guid id)
    {
        using var response = await client.GetWithTokenAsync(ProfilePath(id), token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.ReadJsonAsync(), response.Headers.ETag.ShouldNotBeNull().Tag);
    }
}
