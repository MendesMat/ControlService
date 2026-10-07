using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class SignInApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    [Fact]
    public async Task Sign_in_returns_200_with_an_access_token_and_the_refresh_cookie() // AUTH-07, AUTH-17
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();

        using var response = await client.SignInAsync(user.Login, user.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("accessToken").GetString().ShouldNotBeNullOrEmpty();
        body.GetProperty("expiresIn").GetInt32().ShouldBe(900);
        body.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        body.EnumerateObject().Select(property => property.Name).ShouldBe(["accessToken", "expiresIn", "mustChangePassword"], ignoreOrder: true);

        var cookie = response.RefreshCookie();
        cookie.ShouldNotBeNull();
        cookie.Value.Value.ShouldNotBeNullOrEmpty();
        cookie.HttpOnly.ShouldBeTrue();
        cookie.Secure.ShouldBeTrue();
        cookie.SameSite.ShouldBe(SameSiteMode.Strict);
        cookie.Path.Value.ShouldBe(AuthHttp.RefreshPath);
        cookie.Expires.ShouldBe(factory.Clock.GetUtcNow().AddHours(8));
    }

    [Fact]
    public async Task Access_token_carries_the_user_id_in_sub_and_the_session_in_sid()
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var response = await client.SignInAsync(user.Login, user.Password);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("accessToken").GetString());

        token.GetClaim("sub").Value.ShouldBe(user.Id.ToString());
        Guid.TryParse(token.GetClaim("sid").Value, out _).ShouldBeTrue();
        token.Claims.ShouldNotContain(claim => claim.Type == "must_change_password");
        (token.ValidTo - token.IssuedAt).ShouldBe(TimeSpan.FromSeconds(900));
    }

    [Fact]
    public async Task Wrong_password_returns_401_invalid_credentials() // AUTH-08
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();

        using var response = await client.SignInAsync(user.Login, "senha-errada-123");

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials", "Login ou senha incorretos.");
    }

    [Fact]
    public async Task Fifth_wrong_password_returns_429_locked_out_with_retry_after() // AUTH-08
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            using var failure = await client.SignInAsync(user.Login, "senha-errada-123");
            failure.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        using var response = await client.SignInAsync(user.Login, "senha-errada-123");

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.TooManyRequests, "locked_out", "Muitas tentativas sem sucesso. Aguarde 15 minutos e tente de novo.");
        response.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldBe(TimeSpan.FromSeconds(900));
        problem.GetProperty("details").GetProperty("retryAfterSeconds").GetInt32().ShouldBe(900);
    }

    [Fact]
    public async Task Deactivated_account_returns_403_account_inactive() // AUTH-09
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, user.Id);
        using var client = factory.CreateHttpsClient();

        using var response = await client.SignInAsync(user.Login, user.Password);

        await response.ShouldBeProblemAsync(
            HttpStatusCode.Forbidden, "account_inactive", "Este acesso está desativado. Fale com o responsável pelo sistema.");
    }
}
