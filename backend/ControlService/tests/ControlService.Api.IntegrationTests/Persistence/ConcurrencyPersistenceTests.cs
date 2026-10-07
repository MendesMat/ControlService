using ControlService.Application.Common;
using ControlService.Domain.Access;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class ConcurrencyPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Saving_over_a_change_made_by_someone_else_returns_concurrency_conflict() // CNV-13, CNV-14
    {
        var fabio = User.Create(
            Login.Create("fabio.nunes").Value, EmailAddress.Create("fabio.nunes@example.com").Value, "Fabio Nunes", "Fabio Nunes");
        Guid brunoId;

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            brunoId = (await GetOrCreateBrunoAsync(db, TestContext.Current.CancellationToken)).Id;
            db.Users.Add(fabio);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var scopeA = factory.Services.CreateAsyncScope();
        var dbA = scopeA.ServiceProvider.GetRequiredService<AppDbContext>();
        var fabioA = await dbA.Users.SingleAsync(u => u.Id == fabio.Id, TestContext.Current.CancellationToken);

        await using var scopeB = factory.Services.CreateAsyncScope();
        var dbB = scopeB.ServiceProvider.GetRequiredService<AppDbContext>();
        var fabioB = await dbB.Users.SingleAsync(u => u.Id == fabio.Id, TestContext.Current.CancellationToken);

        factory.CurrentUser.UserId = brunoId;
        fabioA.Activate(DateTimeOffset.UtcNow);
        await dbA.SaveChangesAsync(TestContext.Current.CancellationToken);

        factory.CurrentUser.UserId = SystemIds.AdminUser;
        fabioB.Deactivate(SystemIds.AdminUser, DateTimeOffset.UtcNow);
        var unitOfWorkB = scopeB.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var result = await unitOfWorkB.SaveChangesAsync(TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("concurrency_conflict");
        result.Error.Message.ShouldBe(
            "Este cadastro foi alterado por Bruno Lima enquanto você editava. Recarregue para ver a versão atual.");
        result.Error.Details.ShouldNotBeNull();
        result.Error.Details["updatedByName"].ShouldBe("Bruno Lima");
    }

    [Fact]
    public async Task A_refused_save_writes_nothing() // CNV-13
    {
        var gustavo = User.Create(
            Login.Create("gustavo.reis").Value, EmailAddress.Create("gustavo.reis@example.com").Value, "Gustavo Reis", "Gustavo Reis");
        Guid brunoId;

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            brunoId = (await GetOrCreateBrunoAsync(db, TestContext.Current.CancellationToken)).Id;
            db.Users.Add(gustavo);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scopeA = factory.Services.CreateAsyncScope())
        await using (var scopeB = factory.Services.CreateAsyncScope())
        {
            var dbA = scopeA.ServiceProvider.GetRequiredService<AppDbContext>();
            var gustavoA = await dbA.Users.SingleAsync(u => u.Id == gustavo.Id, TestContext.Current.CancellationToken);
            var dbB = scopeB.ServiceProvider.GetRequiredService<AppDbContext>();
            var gustavoB = await dbB.Users.SingleAsync(u => u.Id == gustavo.Id, TestContext.Current.CancellationToken);

            factory.CurrentUser.UserId = brunoId;
            gustavoA.Activate(DateTimeOffset.UtcNow);
            await dbA.SaveChangesAsync(TestContext.Current.CancellationToken);

            factory.CurrentUser.UserId = SystemIds.AdminUser;
            gustavoB.Deactivate(SystemIds.AdminUser, DateTimeOffset.UtcNow);
            var unitOfWorkB = scopeB.ServiceProvider.GetRequiredService<IUnitOfWork>();
            (await unitOfWorkB.SaveChangesAsync(TestContext.Current.CancellationToken)).IsFailure.ShouldBeTrue();
        }

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await verifyDb.Users.SingleAsync(u => u.Id == gustavo.Id, TestContext.Current.CancellationToken);

        saved.Status.ShouldBe(UserStatus.Active);
        saved.DeactivatedAt.ShouldBeNull();
        saved.DeactivatedBy.ShouldBeNull();
        saved.UpdatedBy.ShouldBe(brunoId);
    }

    [Fact]
    public async Task Saving_levels_over_a_change_made_by_someone_else_returns_concurrency_conflict() // CNV-13, CNV-14
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var profile = PermissionProfile.Create("Compras", "Equipe de compras");
        profile.SetLevel(users, AccessLevel.Editor);
        Guid brunoId;

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            brunoId = (await GetOrCreateBrunoAsync(db, TestContext.Current.CancellationToken)).Id;
            db.PermissionProfiles.Add(profile);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scopeA = factory.Services.CreateAsyncScope())
        await using (var scopeB = factory.Services.CreateAsyncScope())
        {
            var dbA = scopeA.ServiceProvider.GetRequiredService<AppDbContext>();
            var profileA = await dbA.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);
            var dbB = scopeB.ServiceProvider.GetRequiredService<AppDbContext>();
            var profileB = await dbB.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);

            factory.CurrentUser.UserId = brunoId;
            profileA.SetLevel(users, AccessLevel.Reader);
            await dbA.SaveChangesAsync(TestContext.Current.CancellationToken);

            factory.CurrentUser.UserId = SystemIds.AdminUser;
            profileB.SetLevel(users, AccessLevel.Denied);
            var unitOfWorkB = scopeB.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var result = await unitOfWorkB.SaveChangesAsync(TestContext.Current.CancellationToken);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("concurrency_conflict");
            result.Error.Message.ShouldBe(
                "Este cadastro foi alterado por Bruno Lima enquanto você editava. Recarregue para ver a versão atual.");
        }

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await verifyDb.PermissionProfiles.SingleAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken);

        saved.GetLevel(users).ShouldBe(AccessLevel.Reader);
        saved.UpdatedBy.ShouldBe(brunoId);
    }

    // Bruno Lima is the conflict's actor, and must exist in `users` because the authorship columns are
    // foreign keys to it. The display name is unique across the shared database, so both tests reuse
    // the same row instead of each creating their own "Bruno Lima".
    private static async Task<User> GetOrCreateBrunoAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var normalizedDisplayName = TextNormalization.Normalize("Bruno Lima");
        var existing = await db.Users.FirstOrDefaultAsync(
            u => u.NormalizedDisplayName == normalizedDisplayName, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var bruno = User.Create(
            Login.Create("bruno.lima.cc").Value, EmailAddress.Create("bruno.lima.cc@example.com").Value, "Bruno Lima", "Bruno Lima");
        db.Users.Add(bruno);
        await db.SaveChangesAsync(cancellationToken);
        return bruno;
    }
}
