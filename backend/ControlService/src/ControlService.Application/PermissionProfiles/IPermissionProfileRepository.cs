using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles;

public interface IPermissionProfileRepository
{
    /// <summary>The profiles that exist among the given ids; an unknown id is ignored (PERM-05).</summary>
    Task<IReadOnlyList<PermissionProfile>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    /// <summary>The profile whose normalized name is the given one (CNV-09), not tracked, or null.</summary>
    Task<PermissionProfile?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    /// <summary>The profile with its levels, tracked so that changes to it are saved, or null. The saving is checked
    /// against `expectedVersion`, the version the person read (CNV-13), so a handler holds no concurrency code.</summary>
    Task<PermissionProfile?> GetForUpdateAsync(Guid id, uint expectedVersion, CancellationToken cancellationToken);

    void Add(PermissionProfile profile);

    /// <summary>The profile with its levels, not tracked, or null when no profile has the id.</summary>
    Task<PermissionProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Every profile, not tracked, with its levels.</summary>
    Task<IReadOnlyList<PermissionProfile>> ListAllAsync(CancellationToken cancellationToken);
}
