using System.Net;
using ControlService.Domain.Access;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class MeApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string SessionEnded = "Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas.";

    [Fact]
    public async Task Me_returns_the_signed_in_person_and_their_levels() // Operations, D2
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.GrantAsync(factory.Services, user.Id, ScreenKeys.Users, AccessLevel.Editor);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);

        using var response = await client.GetWithTokenAsync("/api/v1/me", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var me = await response.ReadJsonAsync();
        me.GetProperty("id").GetGuid().ShouldBe(user.Id);
        me.GetProperty("displayName").GetString().ShouldBe(user.DisplayName);
        me.GetProperty("login").GetString().ShouldBe(user.Login);
        var levels = me.GetProperty("levels").EnumerateArray().ToList();
        levels.Select(entry => entry.GetProperty("screen").GetString()).ShouldBe(ScreenKeys.All);
        levels.Single(entry => entry.GetProperty("screen").GetString() == ScreenKeys.Users).GetProperty("level").GetString().ShouldBe("editor");
        levels.Where(entry => entry.GetProperty("screen").GetString() != ScreenKeys.Users)
            .ShouldAllBe(entry => entry.GetProperty("level").GetString() == "negado");
    }

    [Fact]
    public async Task Me_without_a_token_returns_401_session_expired() // AUTH-27, T7
    {
        using var client = factory.CreateHttpsClient();

        using var response = await client.GetWithTokenAsync("/api/v1/me", accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }

    [Fact]
    public async Task Expired_access_token_returns_401_session_expired() // AUTH-17
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);
        factory.Clock.Advance(TimeSpan.FromMinutes(15));

        using var response = await client.GetWithTokenAsync("/api/v1/me", token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }
}
