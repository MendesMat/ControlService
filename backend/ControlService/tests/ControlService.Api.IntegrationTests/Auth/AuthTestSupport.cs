using ControlService.Api.IntegrationTests.Common;
using ControlService.Domain.Access;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
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

    /// <summary>Puts the Admin back as the seeder creates it: initial password, mandatory change, no lockout,
    /// no sessions and no activation time. The Admin is one row shared by the whole assembly.</summary>
    public static async Task ResetAdminAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var credential = await db.Set<UserCredential>().SingleAsync(candidate => candidate.Id == SystemIds.AdminUser);
        var hash = new PasswordHasher<UserCredential>().HashPassword(credential, ApiFactory.AdminInitialPassword);

        await db.Set<UserSession>().Where(session => session.UserId == SystemIds.AdminUser).ExecuteDeleteAsync();
        await db.Set<UserCredential>().Where(candidate => candidate.Id == SystemIds.AdminUser).ExecuteUpdateAsync(setters => setters
            .SetProperty(candidate => candidate.PasswordHash, hash)
            .SetProperty(candidate => candidate.MustChangePassword, true)
            .SetProperty(candidate => candidate.AccessFailedCount, 0)
            .SetProperty(candidate => candidate.LockoutEnd, (DateTimeOffset?)null));
        await db.Users.Where(user => user.Id == SystemIds.AdminUser)
            .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.ActivatedAt, (DateTimeOffset?)null));
    }

    public static async Task DeactivateUserAsync(IServiceProvider services, Guid userId)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(candidate => candidate.Id == userId);
        user.Deactivate(SystemIds.AdminUser, DateTimeOffset.UtcNow).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync();
    }

    /// <summary>Gives the user a new profile (unique name, so profile uniqueness never collides) with one level.</summary>
    public static async Task GrantAsync(IServiceProvider services, Guid userId, string screen, AccessLevel level)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profile = PermissionProfile.Create($"Perfil {Guid.NewGuid():N}", "Perfil de teste");
        profile.SetLevel(ScreenKey.Create(screen).Value, level).IsSuccess.ShouldBeTrue();
        db.PermissionProfiles.Add(profile);
        var user = await db.Users.SingleAsync(candidate => candidate.Id == userId);
        user.AssignProfiles([.. user.ProfileIds, profile.Id]).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync();
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
