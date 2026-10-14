using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles.GetProfile;

public sealed class GetProfileHandler(
    IPermissionProfileRepository profiles,
    IUserRepository users) : IQueryHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdAsync(query.Id, cancellationToken);
        if (profile is null)
        {
            return Result<ProfileResponse>.Failure(CommonErrors.NotFound);
        }

        var displayNames = await users.GetDisplayNamesAsync([profile.CreatedBy, profile.UpdatedBy], cancellationToken);
        return Result<ProfileResponse>.Success(profile.ToResponse(displayNames));
    }
}
