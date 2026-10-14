using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class DeleteProfileApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #10-T32
    [Fact]
    public async Task A_manager_deletes_a_profile_nobody_has_with_the_current_version() // PERM-20
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Para excluir {ProfileTestSupport.UniqueSuffix()}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);
        var etag = await ReadETagAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(profileId), token, ifMatch: etag);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var afterwards = await client.GetWithTokenAsync(ProfilePath(profileId), token);
        afterwards.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // #10-T33
    [Fact]
    public async Task A_profile_in_use_is_refused_naming_everyone_who_has_it() // PERM-20
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Em uso {ProfileTestSupport.UniqueSuffix()}");
        var active = await AuthTestSupport.CreateUserAsync(factory.Services);
        var deactivated = await AuthTestSupport.CreateUserAsync(factory.Services);
        await ProfileTestSupport.AssignProfileAsync(factory.Services, active.Id, profileId);
        await ProfileTestSupport.AssignProfileAsync(factory.Services, deactivated.Id, profileId);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, deactivated.Id);
        string[] names = [.. new[] { active.DisplayName, deactivated.DisplayName }.Order(StringComparer.Ordinal)];
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);
        var etag = await ReadETagAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(profileId), token, ifMatch: etag);

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict,
            "profile_in_use",
            $"{names[0]}, {names[1]} usa(m) este perfil. Tire o perfil dessa(s) pessoa(s) na tela Usuários e depois volte para excluir.");
        problem.GetProperty("details").GetProperty("userNames").EnumerateArray().Select(name => name.GetString()).ShouldBe(names);
        (await ProfileTestSupport.ProfileExistsAsync(factory.Services, profileId)).ShouldBeTrue();
    }

    // #10-T34
    [Fact]
    public async Task The_manager_profile_cannot_be_deleted() // PERM-22, PERM-25
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);
        var etag = await ReadETagAsync(client, token, SystemIds.ManagerProfile);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(SystemIds.ManagerProfile), token, ifMatch: etag);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "system_record", UpdateProfileApiTests.SystemRecordMessage);
        (await ProfileTestSupport.ProfileExistsAsync(factory.Services, SystemIds.ManagerProfile)).ShouldBeTrue();
    }

    // #10-T35
    [Fact]
    public async Task Deleting_with_an_old_version_is_refused_and_the_profile_stays() // CNV-13
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Versão antiga {ProfileTestSupport.UniqueSuffix()}");
        using var client = factory.CreateHttpsClient();
        var (_, managerToken) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);
        var (editor, editorToken) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var oldVersion = await ReadETagAsync(client, managerToken, profileId);
        using var changed = await client.SendWithTokenAsync(
            HttpMethod.Put, ProfilePath(profileId), editorToken, new { name = $"Mudou {ProfileTestSupport.UniqueSuffix()}", description = "Mudou" }, oldVersion);
        changed.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(profileId), managerToken, ifMatch: oldVersion);

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "concurrency_conflict", UpdateProfileApiTests.ChangedBy(editor.DisplayName));
        problem.GetProperty("details").GetProperty("updatedByName").GetString().ShouldBe(editor.DisplayName);
        (await ProfileTestSupport.ProfileExistsAsync(factory.Services, profileId)).ShouldBeTrue();
    }

    // #10-T36
    [Fact]
    public async Task Deleting_an_unknown_profile_answers_not_found() // API-16
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(Guid.NewGuid()), token, ifMatch: "\"1\"");

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "not_found", "Registro não encontrado.");
    }

    // #10-T37
    [Fact]
    public async Task An_editor_on_permissions_cannot_delete_a_profile() // PERM-03
    {
        var profileId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Protegido {ProfileTestSupport.UniqueSuffix()}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Editor);
        var etag = await ReadETagAsync(client, token, profileId);

        using var response = await client.SendWithTokenAsync(HttpMethod.Delete, ProfilePath(profileId), token, ifMatch: etag);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
        (await ProfileTestSupport.ProfileExistsAsync(factory.Services, profileId)).ShouldBeTrue();
    }

    private static string ProfilePath(Guid id) => $"{ProfileTestSupport.ProfilesPath}/{id}";

    private static async Task<string> ReadETagAsync(HttpClient client, string token, Guid id)
    {
        using var response = await client.GetWithTokenAsync(ProfilePath(id), token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return response.Headers.ETag.ShouldNotBeNull().Tag;
    }
}
