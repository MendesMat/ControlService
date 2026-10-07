using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

/// <summary>Inserts the Admin, its credential and the Gerenciador profile only when missing, so
/// restarting the application never duplicates or changes them (the Admin comes
/// first, since the profile's authorship references it). The credential is checked apart from the
/// Admin row, so databases created before authentication get it too.</summary>
public static class SystemRecordsSeeder
{
    public static void Seed(DbContext context, string adminEmail, string adminInitialPassword)
    {
        var db = (AppDbContext)context;
        AddMissingRecords(db, adminEmail, adminInitialPassword,
            db.Users.Any(user => user.Id == SystemIds.AdminUser),
            db.Set<UserCredential>().Any(credential => credential.Id == SystemIds.AdminUser),
            db.PermissionProfiles.Any(profile => profile.Id == SystemIds.ManagerProfile));
        db.SaveChanges();
    }

    public static async Task SeedAsync(
        DbContext context, string adminEmail, string adminInitialPassword, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        AddMissingRecords(db, adminEmail, adminInitialPassword,
            await db.Users.AnyAsync(user => user.Id == SystemIds.AdminUser, cancellationToken),
            await db.Set<UserCredential>().AnyAsync(credential => credential.Id == SystemIds.AdminUser, cancellationToken),
            await db.PermissionProfiles.AnyAsync(profile => profile.Id == SystemIds.ManagerProfile, cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddMissingRecords(
        AppDbContext db, string adminEmail, string adminInitialPassword,
        bool hasAdmin, bool hasAdminCredential, bool hasManagerProfile)
    {
        if (!hasAdmin)
        {
            db.Users.Add(User.CreateAdmin(EmailAddress.Create(adminEmail).Value));
        }

        if (!hasAdminCredential)
        {
            db.Set<UserCredential>().Add(CreateAdminCredential(adminInitialPassword));
        }

        if (!hasManagerProfile)
        {
            db.PermissionProfiles.Add(PermissionProfile.CreateManagerProfile());
        }
    }

    private static UserCredential CreateAdminCredential(string initialPassword)
    {
        var id = SystemIds.AdminUser.ToString();
        var credential = new UserCredential
        {
            Id = SystemIds.AdminUser,
            UserName = id,
            NormalizedUserName = id.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
            MustChangePassword = true,
        };
        credential.PasswordHash = new PasswordHasher<UserCredential>().HashPassword(credential, initialPassword);
        return credential;
    }
}
