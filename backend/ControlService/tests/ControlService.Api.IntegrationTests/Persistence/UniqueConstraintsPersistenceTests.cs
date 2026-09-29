using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class UniqueConstraintsPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Login_already_used_by_the_admin_is_refused_ignoring_case() // USR-06
    {
        var user = User.Create(
            Login.Create("ADMIN").Value, EmailAddress.Create("outro.admin@example.com").Value, "Outro Admin", "Outro Admin");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);

        var exception = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe("23505");
        postgresException.ConstraintName.ShouldBe("ix_users_login");
    }

    [Fact]
    public async Task Display_name_equal_after_normalization_is_refused() // USR-03, CNV-09
    {
        var first = User.Create(
            Login.Create("marcia.melo").Value, EmailAddress.Create("marcia.melo@example.com").Value, "Marcia Melo", "Marcia Alves Melo");
        var second = User.Create(
            Login.Create("marcia.melo2").Value, EmailAddress.Create("marcia.melo2@example.com").Value, " márcia melo", "Marcia Alves Melo");

        await using var firstScope = factory.Services.CreateAsyncScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();
        firstDb.Users.Add(first);
        await firstDb.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var secondScope = factory.Services.CreateAsyncScope();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<AppDbContext>();
        secondDb.Users.Add(second);

        var exception = await Should.ThrowAsync<DbUpdateException>(() => secondDb.SaveChangesAsync(TestContext.Current.CancellationToken));
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe("23505");
        postgresException.ConstraintName.ShouldBe("ix_users_display_name_normalized");
    }

    [Fact]
    public async Task Profile_name_equal_to_gerenciador_after_normalization_is_refused() // PERM-17, CNV-09
    {
        var profile = PermissionProfile.Create("GERENCIADOR", "Cópia indevida");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PermissionProfiles.Add(profile);

        var exception = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe("23505");
        postgresException.ConstraintName.ShouldBe("ix_permission_profiles_name_normalized");
    }
}
