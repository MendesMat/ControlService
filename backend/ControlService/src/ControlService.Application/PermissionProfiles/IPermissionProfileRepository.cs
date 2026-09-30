using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles;

public interface IPermissionProfileRepository
{
    /// <summary>The profiles that exist among the given ids; an unknown id is ignored (PERM-05).</summary>
    Task<IReadOnlyList<PermissionProfile>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
