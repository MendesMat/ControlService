using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Api.IntegrationTests.PermissionProfiles;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.Users;

public sealed class GetUserApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #11-T20
    [Fact]
    public async Task A_reader_opens_a_user_with_the_version_and_the_names_of_who_acted() // API-03, API-06
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        using var client = factory.CreateHttpsClient();
        var (editor, editorToken) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        using var created = await client.SendWithTokenAsync(
            HttpMethod.Post, UserTestSupport.UsersPath, editorToken, UserTestSupport.NewUserBody(marker));
        var id = (await created.ReadJsonAsync()).GetProperty("id").GetGuid();
        var (_, readerToken) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{UserTestSupport.UsersPath}/{id}", readerToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.ReadJsonAsync();
        user.GetProperty("id").GetGuid().ShouldBe(id);
        user.GetProperty("login").GetString().ShouldBe($"ana.{marker}");
        user.GetProperty("createdByName").GetString().ShouldBe(editor.DisplayName);
        user.GetProperty("updatedByName").GetString().ShouldBe(editor.DisplayName);
        var version = user.GetProperty("version").GetString();
        version.ShouldNotBeNullOrEmpty();
        response.Headers.ETag.ShouldNotBeNull().Tag.ShouldBe($"\"{version}\"");
    }

    // #11-T21
    [Fact]
    public async Task A_deactivated_user_shows_who_deactivated_and_when() // API-03, USR-17
    {
        var former = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, former.Id);
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{UserTestSupport.UsersPath}/{former.Id}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.ReadJsonAsync();
        user.GetProperty("status").GetString().ShouldBe("inactive");
        DateTimeOffset.TryParse(user.GetProperty("deactivatedAt").GetString(), out _).ShouldBeTrue();
        user.GetProperty("deactivatedBy").GetString().ShouldBe(SystemIds.AdminUser.ToString());
        user.GetProperty("deactivatedByName").GetString().ShouldBe("Admin");
    }

    // #11-T22
    [Fact]
    public async Task The_admin_comes_marked_as_a_system_record_with_the_manager_profile() // USR-23, USR-24
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{UserTestSupport.UsersPath}/{SystemIds.AdminUser}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var admin = await response.ReadJsonAsync();
        admin.GetProperty("isSystem").GetBoolean().ShouldBeTrue();
        admin.GetProperty("profileIds").EnumerateArray().Select(id => id.GetGuid()).ShouldBe([SystemIds.ManagerProfile]);
    }

    // #11-T23
    [Fact]
    public async Task An_unknown_user_id_answers_not_found() // API-16
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{UserTestSupport.UsersPath}/{Guid.NewGuid()}", token);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "not_found", "Registro não encontrado.");
    }

    // #11-T24
    [Fact]
    public async Task Someone_denied_on_users_cannot_open_a_user() // PERM-26
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Manager);

        using var response = await client.GetWithTokenAsync($"{UserTestSupport.UsersPath}/{SystemIds.AdminUser}", token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }
}
