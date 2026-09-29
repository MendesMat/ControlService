using ControlService.Api.IntegrationTests.Common;
using ControlService.Domain.Common;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Persistence;

public sealed class StartupSeedTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Startup_applies_the_schema_and_seeds_the_system_records() // ADR-0022, USR-23, USR-24, PERM-22, PERM-23, CNV-20
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = await db.Users.SingleAsync(u => u.Id == SystemIds.AdminUser, TestContext.Current.CancellationToken);
        admin.Login.Value.ShouldBe("admin");
        admin.DisplayName.ShouldBe("Admin");
        admin.FullName.ShouldBe("Administrador do sistema");
        admin.Email.Value.ShouldBe("admin@example.com");
        admin.Status.ShouldBe(UserStatus.Active);
        admin.IsSystem.ShouldBeTrue();
        admin.ProfileIds.ShouldBe([SystemIds.ManagerProfile]);
        admin.ActivatedAt.ShouldBeNull();
        admin.CreatedBy.ShouldBe(SystemIds.AdminUser);
        admin.UpdatedBy.ShouldBe(SystemIds.AdminUser);

        var manager = await db.PermissionProfiles.SingleAsync(p => p.Id == SystemIds.ManagerProfile, TestContext.Current.CancellationToken);
        manager.Name.ShouldBe("Gerenciador");
        manager.Description.ShouldBe("Acesso total a todas as telas do sistema.");
        manager.IsSystem.ShouldBeTrue();
        manager.Levels.ShouldBeEmpty();
        manager.CreatedBy.ShouldBe(SystemIds.AdminUser);
        manager.UpdatedBy.ShouldBe(SystemIds.AdminUser);
    }
}
