using System.Net;
using ControlService.Api.IntegrationTests.Common;
using ControlService.Domain.Common;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class AdminFirstAccessApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string NewPassword = "nova-senha-admin-1";
    private const string ChangeRequired = "Por segurança, a senha inicial precisa ser trocada no primeiro acesso.";

    [Fact]
    public async Task Admin_with_the_initial_password_signs_in_with_must_change_password() // AUTH-13, AUTH-14
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();

        using var response = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.ReadJsonAsync();
        body.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("accessToken").GetString());
        token.GetClaim("must_change_password").Value.ShouldBe("true");
    }

    [Fact]
    public async Task Admin_with_the_initial_password_gets_401_password_change_required_on_another_endpoint() // AUTH-14, AUTH-27, ADR-0032
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await SignInAsAdminAsync(client);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.ProtectedPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "password_change_required", ChangeRequired);
    }

    [Fact]
    public async Task Admin_with_the_initial_password_can_use_me_and_sign_out() // ADR-0032
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await SignInAsAdminAsync(client);

        using var me = await client.GetWithTokenAsync("/api/v1/me", token);
        using var signOut = await client.PostWithTokenAsync("/api/v1/auth/sign-out", token);

        me.StatusCode.ShouldBe(HttpStatusCode.OK);
        signOut.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Change_password_returns_new_tokens_and_unlocks_other_endpoints() // AUTH-14, USR-34, D3 of #7
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var oldToken = await SignInAsAdminAsync(client);

        using var changed = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", oldToken, new { password = NewPassword, passwordConfirmation = NewPassword });

        changed.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await changed.ReadJsonAsync();
        body.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        changed.RefreshCookie().ShouldNotBeNull();
        var newToken = body.GetProperty("accessToken").GetString();

        using var protectedCall = await client.GetWithTokenAsync(AuthApiFactory.ProtectedPath, newToken);
        protectedCall.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var oldPassword = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);
        oldPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var newPassword = await client.SignInAsync("admin", NewPassword);
        newPassword.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var scope = factory.Services.CreateAsyncScope();
        var admin = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .AsNoTracking().SingleAsync(user => user.Id == SystemIds.AdminUser, TestContext.Current.CancellationToken);
        admin.ActivatedAt.ShouldBe(factory.Clock.GetUtcNow());
        admin.UpdatedBy.ShouldBe(SystemIds.AdminUser);
    }

    [Fact]
    public async Task Change_password_ends_the_other_sessions() // ADR-0019
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        using var other = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);
        var otherCookie = other.RefreshCookie()!.Value.Value;
        var token = await SignInAsAdminAsync(client);

        using var changed = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", token, new { password = NewPassword, passwordConfirmation = NewPassword });
        changed.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var refreshed = await client.RefreshAsync(otherCookie);
        refreshed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await refreshed.ReadJsonAsync()).GetProperty("code").GetString().ShouldBe("session_expired");
    }

    [Fact]
    public async Task Change_password_with_invalid_fields_returns_400_with_field_messages() // AUTH-16, API-15
    {
        await AuthTestSupport.ResetAdminAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await SignInAsAdminAsync(client);

        using var response = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", token, new { password = "1234567", passwordConfirmation = "7654321" });

        var problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest, "validation_failed", "Alguns campos precisam ser corrigidos.");
        var errors = problem.GetProperty("errors");
        errors.GetProperty("password").EnumerateArray().Select(message => message.GetString())
            .ShouldBe(["A senha precisa ter pelo menos 8 caracteres."]);
        errors.GetProperty("passwordConfirmation").EnumerateArray().Select(message => message.GetString())
            .ShouldBe(["As duas senhas não são iguais. Digite de novo."]);
    }

    [Fact]
    public async Task Change_password_without_a_mandatory_change_returns_403_forbidden() // AUTH-24
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);

        using var response = await client.PostWithTokenAsync(
            "/api/v1/auth/change-password", token, new { password = NewPassword, passwordConfirmation = NewPassword });

        await response.ShouldBeProblemAsync(
            HttpStatusCode.Forbidden, "forbidden", "Sua senha já foi criada. Para trocá-la, use “Esqueci minha senha”.");
    }

    private static async Task<string> SignInAsAdminAsync(HttpClient client)
    {
        using var response = await client.SignInAsync("admin", ApiFactory.AdminInitialPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
    }
}
