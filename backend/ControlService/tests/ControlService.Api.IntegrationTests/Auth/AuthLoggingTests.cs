using System.Net;
using ControlService.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class AuthLoggingTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string NewPassword = "nova-senha-admin-1";

    [Fact]
    public async Task Authentication_writes_no_password_or_token_to_the_logs() // AUTH-20, issue Definition of Done
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        var logs = new CapturingLoggerProvider();
        using var logged = factory.WithWebHostBuilder(builder => builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.Services.AddSingleton<ILoggerProvider>(logs);
        }));
        using var client = logged.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
        });

        using var signIn = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        var signInToken = (await signIn.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
        var signInCookie = signIn.RefreshCookie()!.Value.Value!;

        using var refresh = await client.RefreshAsync(signInCookie);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        var refreshToken = (await refresh.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
        var refreshCookie = refresh.RefreshCookie()!.Value.Value!;

        using var change = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", refreshToken, new { password = NewPassword, passwordConfirmation = NewPassword });
        change.StatusCode.ShouldBe(HttpStatusCode.OK);
        var changeToken = (await change.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
        var changeCookie = change.RefreshCookie()!.Value.Value!;

        logs.Lines.ShouldNotBeEmpty();
        string[] secrets =
        [
            ApiFactory.AdminInitialPassword, NewPassword, ApiFactory.SigningKey,
            signInToken, signInCookie, refreshToken, refreshCookie, changeToken, changeCookie,
        ];
        foreach (var secret in secrets)
        {
            logs.Lines.ShouldNotContain(line => line.Contains(secret, StringComparison.Ordinal));
        }
    }
}
