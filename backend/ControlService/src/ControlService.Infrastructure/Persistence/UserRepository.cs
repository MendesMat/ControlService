using ControlService.Application.Users;
using ControlService.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public void Add(User user) => dbContext.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Login == login, cancellationToken);

    // Raw SQL because the login is a value object stored through a converter, which EF cannot search by prefix.
    // `starts_with` takes the prefix literally, so "_" in a login is not read as a wildcard, as it would be in LIKE.
    public async Task<IReadOnlyList<string>> ListLoginsStartingWithAsync(string prefix, CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQuery<string>($"SELECT login AS \"Value\" FROM users WHERE starts_with(login, {prefix})")
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToArray();
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => idList.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListDisplayNamesByProfileAsync(Guid profileId, CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .Where(user => EF.Property<List<ProfileAssignment>>(user, "_profileAssignments").Any(assignment => assignment.ProfileId == profileId))
            .Select(user => user.DisplayName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> CountByProfileAsync(CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .SelectMany(user => EF.Property<List<ProfileAssignment>>(user, "_profileAssignments"))
            .GroupBy(assignment => assignment.ProfileId)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.ProfileId, entry => entry.Count, cancellationToken);
}
