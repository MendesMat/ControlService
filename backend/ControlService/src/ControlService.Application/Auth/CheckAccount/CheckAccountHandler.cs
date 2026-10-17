using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Auth.CheckAccount;

public sealed class CheckAccountHandler(IUserRepository users) : IQueryHandler<CheckAccountQuery, Unit>
{
    public async Task<Result<Unit>> Handle(CheckAccountQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(AuthErrors.SessionExpired);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<Unit>.Failure(AuthErrors.AccountInactive);
        }

        return Result<Unit>.Success(default);
    }
}
