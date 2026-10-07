using System.Net;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class SignOutApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    [Fact]
    public async Task Sign_out_returns_204_expires_the_cookie_and_ends_the_session() // signs out with the token alone: the cookie never reaches this route
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync(user.Login, user.Password);
        var token = (await signIn.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
        var cookie = signIn.RefreshCookie()!.Value.Value;

        using var response = await client.PostWithTokenAsync("/api/v1/auth/sign-out", token);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var expired = response.RefreshCookie();
        expired.ShouldNotBeNull();
        expired.Path.Value.ShouldBe(AuthHttp.RefreshPath);
        expired.Expires.ShouldNotBeNull().ShouldBeLessThan(factory.Clock.GetUtcNow());

        using var refreshed = await client.RefreshAsync(cookie);
        refreshed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await refreshed.ReadJsonAsync()).GetProperty("code").GetString().ShouldBe("session_expired");
    }
}
