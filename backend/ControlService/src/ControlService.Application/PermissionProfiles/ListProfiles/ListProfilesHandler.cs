using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Access;
using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles.ListProfiles;

public sealed class ListProfilesHandler(
    IPermissionProfileRepository profiles,
    IUserRepository users) : IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>>
{
    public async Task<Result<IReadOnlyList<ProfileListItemResponse>>> Handle(
        ListProfilesQuery query, CancellationToken cancellationToken)
    {
        var allProfiles = await profiles.ListAllAsync(cancellationToken);
        var userCounts = await users.CountByProfileAsync(cancellationToken);

        // The normalized name is the comparison of CNV-09, so "álamo" comes before "Bravo".
        IReadOnlyList<ProfileListItemResponse> items = allProfiles
            .OrderBy(profile => profile.NormalizedName, StringComparer.Ordinal)
            .Select(profile => new ProfileListItemResponse(
                profile.Id,
                profile.Name,
                profile.Description,
                profile.IsSystem,
                userCounts.GetValueOrDefault(profile.Id),
                ScreenKeys.All.Count(key => profile.GetLevel(ScreenKey.Create(key).Value) > AccessLevel.Denied),
                ScreenKeys.All.Count))
            .ToArray();

        return Result<IReadOnlyList<ProfileListItemResponse>>.Success(items);
    }
}
