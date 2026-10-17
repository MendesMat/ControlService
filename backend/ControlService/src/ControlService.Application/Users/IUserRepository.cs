using ControlService.Domain.Users;

namespace ControlService.Application.Users;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken);

    /// <summary>The user whose normalized display name is the given one (CNV-09), deactivated ones included, not
    /// tracked, or null.</summary>
    Task<User?> GetByNormalizedDisplayNameAsync(string normalizedDisplayName, CancellationToken cancellationToken);

    /// <summary>The logins, deactivated users included, that start with the prefix (both in lowercase).</summary>
    Task<IReadOnlyList<string>> ListLoginsStartingWithAsync(string prefix, CancellationToken cancellationToken);

    /// <summary>The display name of each user that exists among the given ids.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    /// <summary>The display names of everyone who has the profile, deactivated ones included.</summary>
    Task<IReadOnlyList<string>> ListDisplayNamesByProfileAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>How many users have each profile, deactivated ones included. A profile nobody has is absent.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByProfileAsync(CancellationToken cancellationToken);
}
