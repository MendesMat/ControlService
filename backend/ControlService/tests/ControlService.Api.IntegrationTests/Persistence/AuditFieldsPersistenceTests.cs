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
    public async Task Creating_a_record_fills_authorship_from_the_current_user_and_the_clock() // CNV-10, CNV-19, ADR-0015
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
}
