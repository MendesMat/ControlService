using ControlService.Application.PermissionProfiles;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.Tests.Fakes;

internal sealed class InMemoryPermissionProfileRepository : IPermissionProfileRepository
{
    private readonly List<PermissionProfile> _profiles = [];

    public void Add(PermissionProfile profile) => _profiles.Add(profile);

    public Task<IReadOnlyList<PermissionProfile>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PermissionProfile>>(_profiles.Where(profile => ids.Contains(profile.Id)).ToArray());

    public void Remove(PermissionProfile profile) => _profiles.Remove(profile);

    public Task<PermissionProfile?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        Task.FromResult(_profiles.Find(profile => profile.NormalizedName == normalizedName));

    public Task<PermissionProfile?> GetForUpdateAsync(Guid id, uint expectedVersion, CancellationToken cancellationToken) =>
        Task.FromResult(_profiles.Find(profile => profile.Id == id));

    public Task<PermissionProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_profiles.Find(profile => profile.Id == id));

    public Task<IReadOnlyList<PermissionProfile>> ListAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PermissionProfile>>(_profiles.ToArray());
}
