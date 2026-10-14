using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles.UpdateProfile;

public sealed class UpdateProfileHandler(
    IPermissionProfileRepository profiles,
    IUserRepository users,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfileCommand, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var levels = LevelInput.ParseAll(command.Levels);
        if (levels.IsFailure)
        {
            return Result<ProfileResponse>.Failure(levels.Error);
        }

        var profile = await profiles.GetForUpdateAsync(command.Id, command.Version, cancellationToken);
        if (profile is null)
        {
            return Result<ProfileResponse>.Failure(CommonErrors.NotFound);
        }

        var taken = await profiles.GetByNormalizedNameAsync(TextNormalization.Normalize(command.Name), cancellationToken);
        if (taken is not null && taken.Id != profile.Id)
        {
            return Result<ProfileResponse>.Failure(PermissionProfileErrors.NameTaken(taken.Name));
        }

        var details = profile.ChangeDetails(command.Name, command.Description);
        if (details.IsFailure)
        {
            return Result<ProfileResponse>.Failure(details.Error);
        }

        var replaced = profile.ReplaceLevels(levels.Value);
        if (replaced.IsFailure)
        {
            return Result<ProfileResponse>.Failure(replaced.Error);
        }

        var saved = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return Result<ProfileResponse>.Failure(saved.Error);
        }

        var displayNames = await users.GetDisplayNamesAsync([profile.CreatedBy, profile.UpdatedBy], cancellationToken);
        return Result<ProfileResponse>.Success(profile.ToResponse(displayNames));
    }
}
