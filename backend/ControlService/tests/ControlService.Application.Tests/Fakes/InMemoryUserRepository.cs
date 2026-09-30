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
}
