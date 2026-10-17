using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.Application.Users.GetUser;

public sealed class GetUserHandler(IUserRepository users) : IQueryHandler<GetUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(query.Id, cancellationToken);
        if (user is null)
        {
            return Result<UserResponse>.Failure(CommonErrors.NotFound);
        }

        var displayNames = await users.GetDisplayNamesAsync(user.AuthorIds(), cancellationToken);
        return Result<UserResponse>.Success(user.ToResponse(displayNames));
    }
}
