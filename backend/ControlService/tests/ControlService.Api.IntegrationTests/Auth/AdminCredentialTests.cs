using ControlService.Api.IntegrationTests.Common;
using ControlService.Domain.Common;
using ControlService.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class AdminCredentialTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Startup_creates_the_admin_credential_with_a_mandatory_change() // AUTH-13
    {
        // The first host already created it; removing it lets the next start prove it is created when missing.
        await AuthTestSupport.DeleteAdminCredentialAsync(factory.Services);

        using var restarted = factory.WithWebHostBuilder(_ => { });
        await using var scope = restarted.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<UserCredential>>();

        var credential = await users.FindByIdAsync(SystemIds.AdminUser.ToString());

        credential.ShouldNotBeNull();
        (await users.CheckPasswordAsync(credential, ApiFactory.AdminInitialPassword)).ShouldBeTrue();
        credential.MustChangePassword.ShouldBeTrue();
        credential.UserName.ShouldBe(SystemIds.AdminUser.ToString());
    }

    [Fact]
    public async Task Restarting_does_not_change_the_admin_credential() // AUTH-13, T3
    {
        await AuthTestSupport.DeleteAdminCredentialAsync(factory.Services);
        using var first = factory.WithWebHostBuilder(_ => { });
        _ = first.Services;

        using var restarted = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:InitialPassword"] = "outra-senha-inicial",
            })));
        await using var scope = restarted.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<UserCredential>>();

        var credential = await users.FindByIdAsync(SystemIds.AdminUser.ToString());

        credential.ShouldNotBeNull();
        (await users.CheckPasswordAsync(credential, ApiFactory.AdminInitialPassword)).ShouldBeTrue();
        (await users.CheckPasswordAsync(credential, "outra-senha-inicial")).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)] // the user secret was never set
    [InlineData("")]
    [InlineData("1234567")]
    public void Api_does_not_start_without_a_valid_initial_password(string? invalidPassword) // AUTH-13, AUTH-16
    {
        using var invalid = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:InitialPassword"] = invalidPassword,
            })));

        var exception = Should.Throw<Exception>(() => invalid.Services);

        exception.Message.ShouldContain("Admin:InitialPassword");
        if (!string.IsNullOrEmpty(invalidPassword))
        {
            exception.Message.ShouldNotContain(invalidPassword);
        }
    }
}
