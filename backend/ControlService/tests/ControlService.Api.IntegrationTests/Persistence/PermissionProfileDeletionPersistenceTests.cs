using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class PermissionProfileDeletionPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Profile_assigned_to_a_user_cannot_be_deleted() // PERM-20
    {
        var profile = PermissionProfile.Create("Perfil Em Uso", "Não pode ser excluído");
        var user = User.Create(
            Login.Create("dario.pires").Value, EmailAddress.Create("dario.pires@example.com").Value, "Dario Pires", "Dario Pires");
        user.AssignProfiles([profile.Id]);
        user.Deactivate(SystemIds.AdminUser, DateTimeOffset.UtcNow);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PermissionProfiles.Add(profile);
            db.Users.Add(user);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var deleteScope = factory.Services.CreateAsyncScope();
        var deleteDb = deleteScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tracked = await deleteDb.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);
        deleteDb.PermissionProfiles.Remove(tracked);

        var exception = await Should.ThrowAsync<DbUpdateException>(() => deleteDb.SaveChangesAsync(TestContext.Current.CancellationToken));
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        // PostgreSQL reports an immediate ON DELETE RESTRICT violation as 23001 (restrict_violation),
        // not 23503 (foreign_key_violation, used by NO ACTION): RESTRICT was chosen deliberately (PERM-20, USR-16).
        postgresException.SqlState.ShouldBe("23001");

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.PermissionProfiles.AnyAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task Profile_without_users_can_be_deleted() // PERM-20
    {
        var profile = PermissionProfile.Create("Perfil Sem Uso", "Pode ser excluído");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PermissionProfiles.Add(profile);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tracked = await db.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);
            db.PermissionProfiles.Remove(tracked);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.PermissionProfiles.AnyAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken)).ShouldBeFalse();
    }
}
