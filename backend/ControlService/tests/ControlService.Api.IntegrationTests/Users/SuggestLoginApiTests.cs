using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Api.IntegrationTests.PermissionProfiles;
using ControlService.Domain.Access;

namespace ControlService.Api.IntegrationTests.Users;

public sealed class SuggestLoginApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #11-T03
    [Fact]
    public async Task The_next_free_number_is_suggested_when_the_login_is_taken() // USR-14
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        await AuthTestSupport.CreateUserAsync(factory.Services, login: $"ana.{marker}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, UserTestSupport.SuggestionPath, token, new { fullName = $"Ana Paula {marker}" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.ReadJsonAsync()).GetProperty("login").GetString().ShouldBe($"ana.{marker}2");
    }

    // #11-T04
    [Fact]
    public async Task A_reader_on_users_cannot_ask_for_a_suggestion() // PERM-03, PERM-26
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, UserTestSupport.SuggestionPath, token, new { fullName = "Ana Paula Souza" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }
}
