using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.Users;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Application.Auth.CheckScreenAccess;

public sealed class CheckScreenAccessHandler(
    IUserRepository users,
    IPermissionProfileRepository profiles) : IQueryHandler<CheckScreenAccessQuery, Unit>
{
    public async Task<Result<Unit>> Handle(CheckScreenAccessQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(AuthErrors.SessionExpired);
        }

        var userProfiles = await profiles.ListByIdsAsync(user.ProfileIds, cancellationToken);

        return EffectiveAccess.GetLevel(userProfiles, query.Screen) >= query.Minimum
            ? Result<Unit>.Success(default)
            : Result<Unit>.Failure(AuthErrors.Forbidden);
    }
}
