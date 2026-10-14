using ControlService.Application.PermissionProfiles;
using ControlService.Domain.PermissionProfiles;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

public sealed class PermissionProfileRepository(AppDbContext dbContext) : IPermissionProfileRepository
{
    public async Task<IReadOnlyList<PermissionProfile>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToArray();
        return await dbContext.PermissionProfiles
            .AsNoTracking()
            .Where(profile => idList.Contains(profile.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<PermissionProfile?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.PermissionProfiles.AsNoTracking().SingleOrDefaultAsync(profile => profile.NormalizedName == normalizedName, cancellationToken);

    public async Task<PermissionProfile?> GetForUpdateAsync(Guid id, uint expectedVersion, CancellationToken cancellationToken)
    {
        var profile = await dbContext.PermissionProfiles.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (profile is not null)
        {
            // EF Core compares the original value of the row-version column in the UPDATE and the DELETE. A save
            // that changes no field issues no UPDATE, so the row is marked as changed: every save is checked (CNV-13).
            var entry = dbContext.Entry(profile);
            entry.Property(candidate => candidate.Version).OriginalValue = expectedVersion;
            entry.Property(candidate => candidate.UpdatedAt).IsModified = true;
        }

        return profile;
    }

    public void Add(PermissionProfile profile) => dbContext.PermissionProfiles.Add(profile);

    public Task<PermissionProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PermissionProfiles.AsNoTracking().SingleOrDefaultAsync(profile => profile.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PermissionProfile>> ListAllAsync(CancellationToken cancellationToken) =>
        await dbContext.PermissionProfiles.AsNoTracking().ToListAsync(cancellationToken);
}
