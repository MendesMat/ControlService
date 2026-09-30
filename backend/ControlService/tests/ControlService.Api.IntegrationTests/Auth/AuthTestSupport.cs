using ControlService.Domain.Common;
using ControlService.Infrastructure.Auth;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Auth;

/// <summary>Helpers that reach the database directly. The Admin is one row in a database shared by
/// the whole assembly, so tests that change its credential put it back first.</summary>
internal static class AuthTestSupport
{
    public static async Task DeleteAdminCredentialAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Set<UserSession>().Where(session => session.UserId == SystemIds.AdminUser).ExecuteDeleteAsync();
        await db.Set<UserCredential>().Where(credential => credential.Id == SystemIds.AdminUser).ExecuteDeleteAsync();
    }
}
