using System.Net;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class RateLimitApiTests(RateLimitedApiFactory factory) : IClassFixture<RateLimitedApiFactory>
{
    private const string TooManyRequests = "Muitas tentativas em pouco tempo. Aguarde alguns instantes e tente de novo.";

    [Fact]
    public async Task Sign_in_over_the_rate_limit_returns_429_with_retry_after() // AUTH-26, ADR-0023
    {
        using var client = factory.CreateHttpsClient();
        for (var request = 1; request <= 2; request++)
        {
            using var allowed = await client.SignInAsync("nobody", "any-password-1");
            allowed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        using var response = await client.SignInAsync("nobody", "any-password-1");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.TooManyRequests, "locked_out", TooManyRequests);
        response.Headers.RetryAfter.ShouldNotBeNull();
        problem.GetProperty("details").GetProperty("retryAfterSeconds").GetInt32().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Refresh_over_the_rate_limit_returns_429() // AUTH-26, ADR-0023
    {
        using var client = factory.CreateHttpsClient();
        for (var request = 1; request <= 2; request++)
        {
            using var allowed = await client.RefreshAsync(cookieValue: null);
            allowed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        using var response = await client.RefreshAsync(cookieValue: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.TooManyRequests, "locked_out", TooManyRequests);
        response.Headers.RetryAfter.ShouldNotBeNull();
    }
}
