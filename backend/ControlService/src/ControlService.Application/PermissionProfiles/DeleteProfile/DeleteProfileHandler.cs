using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;

namespace ControlService.Application.PermissionProfiles.DeleteProfile;

public sealed class DeleteProfileHandler(
    IPermissionProfileRepository profiles,
    IUserRepository users,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProfileCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetForUpdateAsync(command.Id, command.Version, cancellationToken);
        if (profile is null)
        {
            return Result<Unit>.Failure(CommonErrors.NotFound);
        }

        var deletable = profile.Delete();
        if (deletable.IsFailure)
        {
            return Result<Unit>.Failure(deletable.Error);
        }

        var userNames = await users.ListDisplayNamesByProfileAsync(profile.Id, cancellationToken);
        if (userNames.Count > 0)
        {
            var alphabetical = userNames.OrderBy(TextNormalization.Normalize, StringComparer.Ordinal).ToArray();
            return Result<Unit>.Failure(PermissionProfileErrors.InUse(alphabetical));
        }

        profiles.Remove(profile);
        var saved = await unitOfWork.SaveChangesAsync(cancellationToken);
        return saved.IsFailure ? Result<Unit>.Failure(saved.Error) : Result<Unit>.Success(default);
    }
}
