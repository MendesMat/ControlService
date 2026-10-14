using System.Net.Http.Headers;
using System.Net.Http.Json;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.PermissionProfiles;

/// <summary>What the profile endpoint tests repeat: people with a level, profiles written straight to the
/// database, and the calls that carry a body or an `If-Match`.</summary>
internal static class ProfileTestSupport
{
    public const string Denied = "Você não tem permissão para fazer isso. Se precisar, fale com o responsável pelo sistema.";

    public const string ProfilesPath = "/api/v1/permission-profiles";

    public const string FieldsToFix = "Alguns campos precisam ser corrigidos.";

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];

    /// <summary>A new active person whose only access is `level` on `screen`, already signed in.</summary>
    public static async Task<(TestUser User, string Token)> SignInWithLevelAsync(
        AuthApiFactory factory, HttpClient client, string screen, AccessLevel level)
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await AuthTestSupport.GrantAsync(factory.Services, user.Id, screen, level);
        return (user, await client.SignInForTokenAsync(user));
    }

    public static async Task<Guid> CreateProfileAsync(
        IServiceProvider services, string name, params (string Screen, AccessLevel Level)[] levels)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profile = PermissionProfile.Create(name, "Perfil de teste");
        foreach (var (screen, level) in levels)
        {
            profile.SetLevel(ScreenKey.Create(screen).Value, level).IsSuccess.ShouldBeTrue();
        }

        db.PermissionProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile.Id;
    }

    public static async Task AssignProfileAsync(IServiceProvider services, Guid userId, Guid profileId)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(candidate => candidate.Id == userId);
        user.AssignProfiles([.. user.ProfileIds, profileId]).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync();
    }

    public static async Task<bool> ProfileExistsAsync(IServiceProvider services, Guid profileId)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.PermissionProfiles.AnyAsync(profile => profile.Id == profileId);
    }

    /// <summary>Sends a request with the access token, an optional JSON body and an optional `If-Match`
    /// (sent exactly as given, so a test can omit the quotes).</summary>
    public static async Task<HttpResponseMessage> SendWithTokenAsync(
        this HttpClient client, HttpMethod method, string path, string accessToken, object? body = null, string? ifMatch = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    /// <summary>The messages a validation_failed response carries under one field.</summary>
    public static string[] FieldErrors(this System.Text.Json.JsonElement problem, string field) =>
        problem.GetProperty("errors").GetProperty(field).EnumerateArray().Select(message => message.GetString()!).ToArray();

    public static async Task<bool> ProfileNameExistsAsync(IServiceProvider services, string name)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.PermissionProfiles.AnyAsync(profile => profile.Name == name);
    }
}
