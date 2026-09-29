using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

/// <summary>Inserts the Admin and the Gerenciador profile only when missing, so restarting the
/// application never duplicates or changes them (ADR-0022, D2, D7: the Admin comes first, since
/// the profile's authorship references it).</summary>
public static class SystemRecordsSeeder
{
    public static void Seed(DbContext context, string adminEmail)
    {
        var db = (AppDbContext)context;
        AddMissingRecords(db, adminEmail, db.Users.Any(user => user.Id == SystemIds.AdminUser),
            db.PermissionProfiles.Any(profile => profile.Id == SystemIds.ManagerProfile));
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, string adminEmail, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        AddMissingRecords(db, adminEmail,
            await db.Users.AnyAsync(user => user.Id == SystemIds.AdminUser, cancellationToken),
            await db.PermissionProfiles.AnyAsync(profile => profile.Id == SystemIds.ManagerProfile, cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddMissingRecords(AppDbContext db, string adminEmail, bool hasAdmin, bool hasManagerProfile)
    {
        if (!hasAdmin)
        {
            db.Users.Add(User.CreateAdmin(EmailAddress.Create(adminEmail).Value));
        }

        if (!hasManagerProfile)
        {
            db.PermissionProfiles.Add(PermissionProfile.CreateManagerProfile());
        }
    }
}
