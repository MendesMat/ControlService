using ControlService.Application.Users;
using ControlService.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Login == login, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToArray();
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => idList.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountByProfileAsync(CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .SelectMany(user => EF.Property<List<ProfileAssignment>>(user, "_profileAssignments"))
            .GroupBy(assignment => assignment.ProfileId)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.ProfileId, entry => entry.Count, cancellationToken);
}
