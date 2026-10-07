using System.Net;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class RefreshApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string SessionEnded = "Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas.";

    [Fact]
    public async Task Refresh_rotates_the_cookie_and_the_old_one_stops_working() // AUTH-17
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync(user.Login, user.Password);
        var oldCookie = signIn.RefreshCookie()!.Value.Value!;

        using var refreshed = await client.RefreshAsync(oldCookie);

        refreshed.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await refreshed.ReadJsonAsync();
        body.GetProperty("accessToken").GetString().ShouldNotBeNullOrEmpty();
        body.GetProperty("expiresIn").GetInt32().ShouldBe(900);
        body.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        var newCookie = refreshed.RefreshCookie();
        newCookie.ShouldNotBeNull();
        newCookie.Value.Value.ShouldNotBe(oldCookie);
        newCookie.Path.Value.ShouldBe(AuthHttp.RefreshPath);

        using var reused = await client.RefreshAsync(oldCookie);
        await reused.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }

    [Fact]
    public async Task Refresh_slides_the_session_for_another_8_hours() // AUTH-17
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync(user.Login, user.Password);
        var cookie = signIn.RefreshCookie()!.Value.Value!;

        factory.Clock.Advance(TimeSpan.FromHours(7));
        using var first = await client.RefreshAsync(cookie);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        factory.Clock.Advance(TimeSpan.FromHours(7));
        using var second = await client.RefreshAsync(first.RefreshCookie()!.Value.Value);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_after_8_idle_hours_returns_401_session_expired() // AUTH-17
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync(user.Login, user.Password);
        factory.Clock.Advance(TimeSpan.FromHours(8));

        using var response = await client.RefreshAsync(signIn.RefreshCookie()!.Value.Value);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }

    [Fact]
    public async Task Refresh_without_a_cookie_returns_401_session_expired() // AUTH-17
    {
        using var client = factory.CreateHttpsClient();

        using var response = await client.RefreshAsync(cookieValue: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }

    [Fact]
    public async Task Refresh_of_a_deactivated_account_returns_401_account_inactive() // AUTH-18, D10
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync(user.Login, user.Password);
        var cookie = signIn.RefreshCookie()!.Value.Value;
        await AuthTestSupport.DeactivateUserAsync(factory.Services, user.Id);

        using var response = await client.RefreshAsync(cookie);
        await response.ShouldBeProblemAsync(
            HttpStatusCode.Unauthorized, "account_inactive", "Este acesso está desativado. Fale com o responsável pelo sistema.");

        using var again = await client.RefreshAsync(cookie);
        await again.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "session_expired", SessionEnded);
    }
}
