using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class ForeignKeyConstraintsPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Authorship_must_point_to_an_existing_user() // USR-16
    {
        // Starts the host (and its seeding, as the Admin) before the current user stops existing.
        _ = factory.Services;
        factory.CurrentUser.UserId = Guid.CreateVersion7();
        var profile = PermissionProfile.Create("Autor Inexistente", "Não deve ser salvo");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PermissionProfiles.Add(profile);

        var exception = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe("23503");

        factory.CurrentUser.UserId = SystemIds.AdminUser;
    }
}
