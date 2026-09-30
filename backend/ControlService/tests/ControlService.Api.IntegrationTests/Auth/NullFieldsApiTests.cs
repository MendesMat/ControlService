using System.Net;
using System.Net.Http.Json;
using ControlService.Api.IntegrationTests.Common;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class NullFieldsApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>, IAsyncLifetime
{
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    // The Admin is one row in a database shared by the whole assembly: whatever a test did to it is undone,
    // so the tests that expect it as seeded (StartupSeedTests) pass in any order.
    public async ValueTask DisposeAsync() => await AuthTestSupport.ResetAdminAsync(factory.Services);

    [Fact]
    public async Task Sign_in_with_null_fields_returns_401_invalid_credentials() // AUTH-08, API-12
    {
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/sign-in", new { login = (string?)null, password = (string?)null }, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials", "Login ou senha incorretos.");
    }

    [Fact]
    public async Task Change_password_with_null_fields_returns_400_validation_failed() // AUTH-16, API-15
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var signIn = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);
        var token = (await signIn.ReadJsonAsync()).GetProperty("accessToken").GetString()!;

        using var response = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", token, new { password = (string?)null, passwordConfirmation = (string?)null });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", "Alguns campos precisam ser corrigidos.");
    }
}
