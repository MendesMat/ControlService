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

    public void Add(PermissionProfile profile) => dbContext.PermissionProfiles.Add(profile);

    public Task<PermissionProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PermissionProfiles.AsNoTracking().SingleOrDefaultAsync(profile => profile.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PermissionProfile>> ListAllAsync(CancellationToken cancellationToken) =>
        await dbContext.PermissionProfiles.AsNoTracking().ToListAsync(cancellationToken);
}
