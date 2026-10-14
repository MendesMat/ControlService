using ControlService.Domain.Users;

namespace ControlService.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken);

    /// <summary>The display name of each user that exists among the given ids.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    /// <summary>How many users have each profile, deactivated ones included. A profile nobody has is absent.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByProfileAsync(CancellationToken cancellationToken);
}
