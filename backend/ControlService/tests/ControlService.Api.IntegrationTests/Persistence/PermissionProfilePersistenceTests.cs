using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class PermissionProfilePersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task Profile_is_saved_and_read_back_with_its_levels() // PERM-05
    {
        var users = ScreenKey.Create(ScreenKeys.Users).Value;
        var customers = ScreenKey.Create(ScreenKeys.Customers).Value;
        var profile = PermissionProfile.Create("Vendedor", "Equipe comercial");
        profile.SetLevel(users, AccessLevel.Editor);
        profile.SetLevel(customers, AccessLevel.Reader);

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

            saved.Levels.ShouldBe([new ScreenLevel(users, AccessLevel.Editor), new ScreenLevel(customers, AccessLevel.Reader)],
                ignoreOrder: true);
            saved.GetLevel(users).ShouldBe(AccessLevel.Editor);
            saved.GetLevel(customers).ShouldBe(AccessLevel.Reader);
            saved.GetLevel(ScreenKey.Create(ScreenKeys.Vehicles).Value).ShouldBe(AccessLevel.Denied);
        }
    }
}
