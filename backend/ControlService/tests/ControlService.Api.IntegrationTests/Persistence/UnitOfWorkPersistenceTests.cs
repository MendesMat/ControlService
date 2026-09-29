using ControlService.Application.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class UnitOfWorkPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Unit_of_work_saves_the_changes() // ADR-0009
    {
        var profile = PermissionProfile.Create("Perfil UoW", "Criado via IUnitOfWork");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        db.PermissionProfiles.Add(profile);

        var result = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.PermissionProfiles.AnyAsync(p => p.Id == profile.Id, TestContext.Current.CancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task Version_changes_on_every_save() // CNV-12
    {
        var user = User.Create(
            Login.Create("elder.teixeira").Value, EmailAddress.Create("elder.teixeira@example.com").Value,
            "Elder Teixeira", "Elder Teixeira");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var firstVersion = user.Version;

        user.Activate(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.Version.ShouldNotBe(firstVersion);
    }
}
