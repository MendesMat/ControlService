using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class GetProfileApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string NotFound = "Registro não encontrado.";

    // #10-T11
    [Fact]
    public async Task A_reader_opens_a_profile_with_the_twenty_screens_the_authorship_and_the_version() // API-03, API-06
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        var profileId = await ProfileTestSupport.CreateProfileAsync(
            factory.Services, $"Vendas {suffix}", (ScreenKeys.Customers, AccessLevel.Editor), (ScreenKeys.AccountsReceivable, AccessLevel.Reader));
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{ProfileTestSupport.ProfilesPath}/{profileId}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.ReadJsonAsync();
        profile.GetProperty("id").GetGuid().ShouldBe(profileId);
        profile.GetProperty("name").GetString().ShouldBe($"Vendas {suffix}");
        profile.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
        var levels = profile.GetProperty("levels").EnumerateArray().ToList();
        levels.Select(entry => entry.GetProperty("screen").GetString()).ShouldBe(ScreenKeys.All);
        levels.Single(entry => entry.GetProperty("screen").GetString() == ScreenKeys.Customers).GetProperty("level").GetString().ShouldBe("editor");
        levels.Single(entry => entry.GetProperty("screen").GetString() == ScreenKeys.AccountsReceivable).GetProperty("level").GetString().ShouldBe("leitor");
        levels.Count(entry => entry.GetProperty("level").GetString() == "negado").ShouldBe(18);
        profile.GetProperty("createdBy").GetGuid().ShouldBe(SystemIds.AdminUser);
        profile.GetProperty("createdByName").GetString().ShouldBe("Admin");
        profile.GetProperty("updatedBy").GetGuid().ShouldBe(SystemIds.AdminUser);
        profile.GetProperty("updatedByName").GetString().ShouldBe("Admin");
        var version = profile.GetProperty("version").GetString();
        version.ShouldNotBeNullOrEmpty();
        response.Headers.ETag.ShouldNotBeNull().Tag.ShouldBe($"\"{version}\"");
    }

    // #10-T12
    [Fact]
    public async Task The_manager_profile_comes_with_manager_on_every_screen() // PERM-23
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{ProfileTestSupport.ProfilesPath}/{SystemIds.ManagerProfile}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.ReadJsonAsync();
        profile.GetProperty("isSystem").GetBoolean().ShouldBeTrue();
        var levels = profile.GetProperty("levels").EnumerateArray().ToList();
        levels.Count.ShouldBe(20);
        levels.ShouldAllBe(entry => entry.GetProperty("level").GetString() == "gerenciador");
    }

    // #10-T13
    [Fact]
    public async Task An_unknown_profile_id_answers_not_found() // API-16
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{ProfileTestSupport.ProfilesPath}/{Guid.NewGuid()}", token);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "not_found", NotFound);
    }

    // #10-T14
    [Fact]
    public async Task A_reader_on_users_only_cannot_open_a_profile() // PERM-04, PERM-26
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync($"{ProfileTestSupport.ProfilesPath}/{SystemIds.ManagerProfile}", token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }
}
