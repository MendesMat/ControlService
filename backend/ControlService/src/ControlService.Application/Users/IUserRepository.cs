using ControlService.Domain.Users;

namespace ControlService.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken);
}
