using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>The calls and readings of the authentication routes that many tests repeat.</summary>
internal static class AuthHttp
{
    public const string RefreshCookieName = "__Secure-cs-refresh";
    public const string RefreshPath = "/api/v1/auth/refresh";

    public static Task<HttpResponseMessage> SignInAsync(this HttpClient client, string login, string password) =>
        client.PostAsJsonAsync("/api/v1/auth/sign-in", new { login, password }, TestContext.Current.CancellationToken);

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

    /// <summary>Asserts the status and the `code` and `message` of a Problem Details response (API-12).</summary>
    public static async Task<JsonElement> ShouldBeProblemAsync(
        this HttpResponseMessage response, HttpStatusCode status, string code, string message)
    {
        response.StatusCode.ShouldBe(status);
        var problem = await response.ReadJsonAsync();
        problem.GetProperty("code").GetString().ShouldBe(code);
        problem.GetProperty("message").GetString().ShouldBe(message);
        return problem;
    }

    public static SetCookieHeaderValue? RefreshCookie(this HttpResponseMessage response) =>
        response.Headers.TryGetValues(HeaderNames.SetCookie, out var values)
            ? values.Select(value => SetCookieHeaderValue.Parse(value)).FirstOrDefault(cookie => cookie.Name == RefreshCookieName)
            : null;
}
