using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;

namespace ControlService.Api.IntegrationTests.Access;

public sealed class ScreenAccessApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string Denied = "Você não tem permissão para fazer isso. Se precisar, fale com o responsável pelo sistema.";

    // #9-T05
    [Fact]
    public async Task A_person_without_profiles_is_refused_on_a_reader_route() // PERM-03, PERM-05
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersReaderPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", Denied);
    }

    // #9-T06
    [Fact]
    public async Task A_reader_is_allowed_on_a_reader_route() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersReaderPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #9-T07
    [Fact]
    public async Task A_reader_is_refused_on_an_editor_route() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersEditorPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", Denied);
    }

    // #9-T08
    [Fact]
    public async Task An_editor_is_allowed_on_an_editor_route() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Editor);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersEditorPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #9-T09
    [Fact]
    public async Task An_editor_is_refused_on_a_manager_route() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Editor);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersManagerPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", Denied);
    }

    // #9-T10
    [Fact]
    public async Task A_manager_is_allowed_on_a_manager_route() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Manager);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersManagerPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #9-T11
    [Fact]
    public async Task A_manager_is_allowed_on_a_reader_route() // PERM-02
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Users, AccessLevel.Manager);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersReaderPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // #9-T12
    [Fact]
    public async Task A_level_on_another_screen_does_not_count() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var token = await SignInWithLevelAsync(client, ScreenKeys.Customers, AccessLevel.Manager);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersReaderPath, token);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", Denied);
    }

    // #9-T13
    [Fact]
    public async Task A_pending_password_change_is_answered_before_an_insufficient_level() // AUTH-30, AUTH-14
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.RequirePasswordChangeAsync(factory.Services, user.Id);
        using var client = factory.CreateHttpsClient();
        var token = await client.SignInForTokenAsync(user);

        using var response = await client.GetWithTokenAsync(AuthApiFactory.UsersReaderPath, token);

        await response.ShouldBeProblemAsync(
            HttpStatusCode.Unauthorized,
            "password_change_required",
            "Por segurança, a senha inicial precisa ser trocada no primeiro acesso.");
    }

    /// <summary>A new person with one level on one screen, signed in.</summary>
    private async Task<string> SignInWithLevelAsync(HttpClient client, string screen, AccessLevel level)
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.GrantAsync(factory.Services, user.Id, screen, level);
        return await client.SignInForTokenAsync(user);
    }
}
