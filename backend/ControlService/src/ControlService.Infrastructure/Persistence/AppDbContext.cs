using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

/// <summary>The domain aggregates plus the Identity tables of the credentials, with no roles:
/// permissions are per screen (ADR-0020).</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityUserContext<UserCredential, Guid>(options)
{
    // Identity's base context already has a `Users` set, of credentials. The domain users keep the
    // name the rest of the code base uses; the credentials are reached through `Set<UserCredential>()`.
    public new DbSet<User> Users => Set<User>();

    public DbSet<PermissionProfile> PermissionProfiles => Set<PermissionProfile>();

    public DbSet<UserSession> Sessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_credential_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_credential_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_credential_tokens");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
