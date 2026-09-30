using ControlService.Domain.Common;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Auth;
using ControlService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>Helpers that reach the database directly. The Admin is one row in a database shared by
/// the whole assembly, so tests that change its credential put it back first.</summary>
internal static class AuthTestSupport
{
    public const string UserPassword = "senha-de-teste-1";

    public static async Task DeleteAdminCredentialAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Set<UserSession>().Where(session => session.UserId == SystemIds.AdminUser).ExecuteDeleteAsync();
        await db.Set<UserCredential>().Where(credential => credential.Id == SystemIds.AdminUser).ExecuteDeleteAsync();
    }

    /// <summary>An active user with a credential, under a login no other test uses, so a test never
    /// collides with the unique indexes of another.</summary>
    public static async Task<TestUser> CreateUserAsync(IServiceProvider services, string password = UserPassword)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var credentials = scope.ServiceProvider.GetRequiredService<UserManager<UserCredential>>();

        var suffix = Guid.NewGuid().ToString("N")[..12];
        var login = $"user{suffix}";
        var user = User.Create(
            Login.Create(login).Value, EmailAddress.Create($"{login}@example.com").Value, $"Pessoa {suffix}", $"Pessoa de Teste {suffix}");
        user.Activate(DateTimeOffset.UtcNow);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var credential = new UserCredential { Id = user.Id, UserName = user.Id.ToString() };
        (await credentials.CreateAsync(credential, password)).Succeeded.ShouldBeTrue();

        return new TestUser(user.Id, login, password, $"Pessoa {suffix}");
    }
}

internal sealed record TestUser(Guid Id, string Login, string Password, string DisplayName);
