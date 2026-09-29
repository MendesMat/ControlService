using ControlService.Domain.Common;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class UserPersistenceTests(PersistenceApiFactory factory) : IClassFixture<PersistenceApiFactory>
{
    [Fact]
    public async Task User_is_saved_and_read_back_with_the_same_values() // CNV-04, ADR-0012
    {
        var login = Login.Create("ana.souza").Value;
        var email = EmailAddress.Create("ana.souza@example.com").Value;
        var user = User.Create(login, email, "Ana Souza", "Ana Paula Souza");
        user.AssignProfiles([SystemIds.ManagerProfile]);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(user);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.Users.SingleAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);

            saved.Id.ShouldBe(user.Id);
            var versionNibble = saved.Id.ToByteArray()[7] >> 4;
            versionNibble.ShouldBe(7);
            saved.Login.Value.ShouldBe("ana.souza");
            saved.Email.Value.ShouldBe("ana.souza@example.com");
            saved.DisplayName.ShouldBe("Ana Souza");
            saved.NormalizedDisplayName.ShouldBe("ana souza");
            saved.FullName.ShouldBe("Ana Paula Souza");
            saved.Status.ShouldBe(UserStatus.Pending);
            saved.ProfileIds.ShouldBe([SystemIds.ManagerProfile]);
        }
    }
}
