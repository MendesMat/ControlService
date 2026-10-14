using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

public sealed class ListProfilesApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #10-T06
    [Fact]
    public async Task A_reader_lists_the_profiles_with_their_counters() // API-08, PERM-20
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        var profileId = await ProfileTestSupport.CreateProfileAsync(
            factory.Services, $"Vendas {suffix}", (ScreenKeys.Users, AccessLevel.Editor), (ScreenKeys.Customers, AccessLevel.Reader));
        var active = await AuthTestSupport.CreateUserAsync(factory.Services);
        var deactivated = await AuthTestSupport.CreateUserAsync(factory.Services);
        await ProfileTestSupport.AssignProfileAsync(factory.Services, active.Id, profileId);
        await ProfileTestSupport.AssignProfileAsync(factory.Services, deactivated.Id, profileId);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, deactivated.Id);
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(ProfileTestSupport.ProfilesPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = (await response.ReadJsonAsync()).EnumerateArray().Single(item => item.GetProperty("id").GetGuid() == profileId);
        profile.GetProperty("name").GetString().ShouldBe($"Vendas {suffix}");
        profile.GetProperty("description").GetString().ShouldBe("Perfil de teste");
        profile.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
        profile.GetProperty("userCount").GetInt32().ShouldBe(2);
        profile.GetProperty("grantedCount").GetInt32().ShouldBe(2);
        profile.GetProperty("screenCount").GetInt32().ShouldBe(20);
    }

    // #10-T07
    [Fact]
    public async Task The_list_has_the_manager_profile_with_every_screen_granted() // PERM-23
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(ProfileTestSupport.ProfilesPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var manager = (await response.ReadJsonAsync()).EnumerateArray()
            .Single(item => item.GetProperty("id").GetGuid() == SystemIds.ManagerProfile);
        manager.GetProperty("name").GetString().ShouldBe("Gerenciador");
        manager.GetProperty("isSystem").GetBoolean().ShouldBeTrue();
        manager.GetProperty("grantedCount").GetInt32().ShouldBe(20);
        manager.GetProperty("screenCount").GetInt32().ShouldBe(20);
    }

    // #10-T08
    [Fact]
    public async Task The_list_is_in_alphabetical_order_ignoring_case_and_accents() // CNV-09
    {
        var suffix = ProfileTestSupport.UniqueSuffix();
        var bravoId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Bravo {suffix}");
        var alamoId = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"álamo {suffix}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.PermissionProfiles, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(ProfileTestSupport.ProfilesPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var ids = (await response.ReadJsonAsync()).EnumerateArray().Select(item => item.GetProperty("id").GetGuid()).ToList();
        ids.IndexOf(alamoId).ShouldBeLessThan(ids.IndexOf(bravoId));
    }

    // #10-T09
    [Fact]
    public async Task A_reader_on_users_only_can_list_the_profiles() // PERM-04
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(ProfileTestSupport.ProfilesPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #10-T10
    [Fact]
    public async Task A_person_without_a_level_on_permissions_or_users_cannot_list_the_profiles() // PERM-26
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Customers, AccessLevel.Manager);

        using var response = await client.GetWithTokenAsync(ProfileTestSupport.ProfilesPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }
}
