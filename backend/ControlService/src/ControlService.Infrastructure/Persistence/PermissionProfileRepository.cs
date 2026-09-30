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
}
