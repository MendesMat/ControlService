using ControlService.Application.PermissionProfiles;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.Tests.Fakes;

internal sealed class InMemoryPermissionProfileRepository : IPermissionProfileRepository
{
    private readonly List<PermissionProfile> _profiles = [];

    public void Add(PermissionProfile profile) => _profiles.Add(profile);

    public Task<IReadOnlyList<PermissionProfile>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PermissionProfile>>(_profiles.Where(profile => ids.Contains(profile.Id)).ToArray());
}
