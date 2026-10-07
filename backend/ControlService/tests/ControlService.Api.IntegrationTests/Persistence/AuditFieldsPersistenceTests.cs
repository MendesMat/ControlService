using ControlService.Domain.Access;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class AuditFieldsPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Creating_a_record_fills_authorship_from_the_current_user_and_the_clock() // CNV-10, CNV-19
    {
        var createdAt = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);
        factory.Clock.Set(createdAt);
        factory.CurrentUser.UserId = SystemIds.AdminUser;
        var profile = PermissionProfile.Create("Financeiro", "Equipe financeira");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PermissionProfiles.Add(profile);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);

            saved.CreatedAt.ShouldBe(createdAt);
            saved.UpdatedAt.ShouldBe(createdAt);
            saved.CreatedBy.ShouldBe(SystemIds.AdminUser);
            saved.UpdatedBy.ShouldBe(SystemIds.AdminUser);
        }
    }

    [Fact]
    public async Task Changing_a_record_updates_only_the_last_change_authorship() // CNV-10
    {
        var createdAt = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 9, 20, 17, 41, 2, TimeSpan.Zero);
        factory.Clock.Set(createdAt);
        factory.CurrentUser.UserId = SystemIds.AdminUser;

        var bruno = User.Create(
            Login.Create("bruno.lima").Value, EmailAddress.Create("bruno.lima@example.com").Value, "Bruno Lima", "Bruno Lima");
        var carla = User.Create(
            Login.Create("carla.dias").Value, EmailAddress.Create("carla.dias@example.com").Value, "Carla Dias", "Carla Dias");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.AddRange(bruno, carla);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        factory.Clock.Set(updatedAt);
        factory.CurrentUser.UserId = bruno.Id;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tracked = await db.Users.SingleAsync(u => u.Id == carla.Id, TestContext.Current.CancellationToken);
            tracked.Activate(updatedAt);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.Users.SingleAsync(u => u.Id == carla.Id, TestContext.Current.CancellationToken);

            saved.CreatedAt.ShouldBe(createdAt);
            saved.CreatedBy.ShouldBe(SystemIds.AdminUser);
            saved.UpdatedAt.ShouldBe(updatedAt);
            saved.UpdatedBy.ShouldBe(bruno.Id);
        }
    }

    [Fact]
    public async Task Changing_only_the_levels_of_a_profile_updates_the_last_change_authorship() // CNV-10, CNV-12
    {
        var createdAt = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 9, 21, 9, 12, 30, TimeSpan.Zero);
        factory.Clock.Set(createdAt);
        factory.CurrentUser.UserId = SystemIds.AdminUser;
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var profile = PermissionProfile.Create("Almoxarifado", "Equipe de estoque");
        profile.SetLevel(users, AccessLevel.Editor);
        uint createdVersion;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PermissionProfiles.Add(profile);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            createdVersion = profile.Version;
        }

        factory.Clock.Set(updatedAt);
        factory.CurrentUser.UserId = await GetOrCreateUserAsync("helena.prado", "Helena Prado");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tracked = await db.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);
            tracked.SetLevel(users, AccessLevel.Reader);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);

            saved.GetLevel(users).ShouldBe(AccessLevel.Reader);
            saved.UpdatedAt.ShouldBe(updatedAt);
            saved.UpdatedBy.ShouldBe(factory.CurrentUser.UserId!.Value);
            saved.Version.ShouldNotBe(createdVersion);
        }
    }

    [Fact]
    public async Task Changing_only_the_profiles_of_a_user_updates_the_last_change_authorship() // CNV-10, CNV-12
    {
        var createdAt = new DateTimeOffset(2026, 9, 12, 13, 5, 44, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 9, 22, 10, 0, 0, TimeSpan.Zero);
        factory.Clock.Set(createdAt);
        factory.CurrentUser.UserId = SystemIds.AdminUser;
        var ivo = User.Create(
            Login.Create("ivo.mattos").Value, EmailAddress.Create("ivo.mattos@example.com").Value, "Ivo Mattos", "Ivo Mattos");
        uint createdVersion;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(ivo);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            createdVersion = ivo.Version;
        }

        factory.Clock.Set(updatedAt);
        factory.CurrentUser.UserId = await GetOrCreateUserAsync("helena.prado", "Helena Prado");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tracked = await db.Users.SingleAsync(u => u.Id == ivo.Id, TestContext.Current.CancellationToken);
            tracked.AssignProfiles([SystemIds.ManagerProfile]);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.Users.SingleAsync(u => u.Id == ivo.Id, TestContext.Current.CancellationToken);

            saved.ProfileIds.ShouldBe([SystemIds.ManagerProfile]);
            saved.UpdatedAt.ShouldBe(updatedAt);
            saved.UpdatedBy.ShouldBe(factory.CurrentUser.UserId!.Value);
            saved.Version.ShouldNotBe(createdVersion);
        }
    }

    [Fact]
    public async Task Saving_during_a_request_without_a_signed_in_person_fails() // CNV-20
    {
        // Starts the host (and its seeding, as the Admin) before removing the current user.
        _ = factory.Services;
        factory.CurrentUser.UserId = null;
        var profile = PermissionProfile.Create("Sem Autor", "Não deve ser salvo");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PermissionProfiles.Add(profile);

        await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));

        factory.CurrentUser.UserId = SystemIds.AdminUser;
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.PermissionProfiles.AnyAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    // The editor must exist in `users`, since the authorship columns are foreign keys to it. Both tests
    // reuse the same row, because the display name is unique across the shared database.
    private async Task<Guid> GetOrCreateUserAsync(string login, string displayName)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existing = await db.Users.FirstOrDefaultAsync(
            u => u.Login == Login.Create(login).Value, TestContext.Current.CancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var user = User.Create(Login.Create(login).Value, EmailAddress.Create($"{login}@example.com").Value, displayName, displayName);
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user.Id;
    }
}
