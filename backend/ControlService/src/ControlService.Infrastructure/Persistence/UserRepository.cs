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
}
