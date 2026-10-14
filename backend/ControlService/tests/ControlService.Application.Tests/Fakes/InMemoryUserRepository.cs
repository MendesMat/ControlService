using ControlService.Application.Users;
using ControlService.Domain.Users;

namespace ControlService.Application.Tests.Fakes;

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public void Add(User user) => _users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Find(user => user.Id == id));

    public Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Find(user => user.Login.Value == login.Value));

    public Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, string>>(
            _users.Where(user => ids.Contains(user.Id)).ToDictionary(user => user.Id, user => user.DisplayName));

    public Task<IReadOnlyList<string>> ListDisplayNamesByProfileAsync(Guid profileId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(
            _users.Where(user => user.ProfileIds.Contains(profileId)).Select(user => user.DisplayName).ToArray());

    public Task<IReadOnlyDictionary<Guid, int>> CountByProfileAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, int>>(_users
            .SelectMany(user => user.ProfileIds)
            .GroupBy(profileId => profileId)
            .ToDictionary(group => group.Key, group => group.Count()));
}
