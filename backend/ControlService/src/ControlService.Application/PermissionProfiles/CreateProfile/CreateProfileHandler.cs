using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.PermissionProfiles.CreateProfile;

public sealed class CreateProfileHandler(
    IPermissionProfileRepository profiles,
    IUserRepository users,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateProfileCommand, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = PermissionProfile.Create(command.Name, command.Description);
        var taken = await profiles.GetByNormalizedNameAsync(profile.NormalizedName, cancellationToken);
        if (taken is not null)
        {
            return Result<ProfileResponse>.Failure(PermissionProfileErrors.NameTaken(taken.Name));
        }

        var applied = profile.ReplaceLevels(LevelInput.ParseAll(command.Levels));
        if (applied.IsFailure)
        {
            return Result<ProfileResponse>.Failure(applied.Error);
        }

        profiles.Add(profile);
        var saved = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return Result<ProfileResponse>.Failure(saved.Error);
        }

        var displayNames = await users.GetDisplayNamesAsync([profile.CreatedBy, profile.UpdatedBy], cancellationToken);
        return Result<ProfileResponse>.Success(profile.ToResponse(displayNames));
    }
}
