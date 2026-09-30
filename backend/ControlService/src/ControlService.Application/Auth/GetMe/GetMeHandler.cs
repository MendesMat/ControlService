using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.Users;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Application.Auth.GetMe;

public sealed class GetMeHandler(
    IUserRepository users,
    IPermissionProfileRepository profiles) : IQueryHandler<GetMeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result<MeResponse>.Failure(AuthErrors.SessionExpired);
        }

        var userProfiles = await profiles.ListByIdsAsync(user.ProfileIds, cancellationToken);

        var levels = ScreenKeys.All
            .Select(key => new ScreenLevelResponse(
                key, EffectiveAccess.GetLevel(userProfiles, ScreenKey.Create(key).Value).ToWireValue()))
            .ToArray();

        return Result<MeResponse>.Success(new MeResponse(user.Id, user.DisplayName, user.Login.Value, levels));
    }
}
