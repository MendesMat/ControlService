using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class AccountCheckApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string AccountInactive = "Este acesso está desativado. Fale com o responsável pelo sistema.";

    // #9-T02
    [Fact]
    public async Task A_deactivated_account_is_refused_on_the_next_request_even_with_a_valid_token() // AUTH-18
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, user.Id);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.ProtectedPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "account_inactive", AccountInactive);
    }

    // #9-T03
    [Theory]
    [InlineData("GET", "/api/v1/me")]
    [InlineData("POST", "/api/v1/auth/sign-out")]
    [InlineData("POST", "/api/v1/auth/change-password")]
    public async Task A_deactivated_account_is_also_refused_on_me_sign_out_and_change_password(string method, string path) // AUTH-18
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, user.Id);
        // The body lets change-password bind, since binding happens before the filters run.
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "account_inactive", AccountInactive);
    }

    // #9-T04
    [Fact]
    public async Task A_deactivated_account_is_refused_as_inactive_before_the_password_change() // AUTH-30
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.RequirePasswordChangeAsync(factory.Services, user.Id);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);
        await AuthTestSupport.DeactivateUserAsync(factory.Services, user.Id);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.ProtectedPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "account_inactive", AccountInactive);
    }
}
