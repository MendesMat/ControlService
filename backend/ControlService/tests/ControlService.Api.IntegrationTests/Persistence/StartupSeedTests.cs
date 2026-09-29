using ControlService.Api.IntegrationTests.Common;
using ControlService.Domain.Common;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    [Fact]
    public async Task Restarting_does_not_duplicate_or_change_the_system_records() // ADR-0022, USR-33
    {
        // Starts the original host first, with admin@example.com, before the "restart" below.
        _ = factory.Services;

        using var restarted = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:Email"] = "other@example.com",
            })));

        await using var scope = restarted.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        (await db.Users.CountAsync(u => u.Id == SystemIds.AdminUser, TestContext.Current.CancellationToken)).ShouldBe(1);
        (await db.PermissionProfiles.CountAsync(p => p.Id == SystemIds.ManagerProfile, TestContext.Current.CancellationToken)).ShouldBe(1);
        var admin = await db.Users.SingleAsync(u => u.Id == SystemIds.AdminUser, TestContext.Current.CancellationToken);
        admin.Email.Value.ShouldBe("admin@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Api_does_not_start_without_a_valid_admin_email(string invalidEmail) // USR-24, local environment guide
    {
        using var invalid = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:Email"] = invalidEmail,
            })));

        var exception = Should.Throw<Exception>(() => invalid.Services);

        exception.Message.ShouldContain("Admin:Email");
    }

    [Fact]
    public void Api_uses_the_system_clock() // ADR-0015
    {
        var timeProvider = factory.Services.GetRequiredService<TimeProvider>();

        timeProvider.ShouldBeSameAs(TimeProvider.System);
    }
}
