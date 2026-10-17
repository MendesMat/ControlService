using ControlService.Application.Auth;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Users.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository users,
    IPermissionProfileRepository profiles,
    ICredentialStore credentials,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = User.Create(
            Login.Create(command.Login).Value,
            EmailAddress.Create(command.Email).Value,
            command.DisplayName,
            command.FullName,
            command.ToPersonalData());

        var loginOwner = await users.GetByLoginAsync(user.Login, cancellationToken);
        var displayNameOwner = await users.GetByNormalizedDisplayNameAsync(user.NormalizedDisplayName, cancellationToken);
        if (loginOwner is not null || displayNameOwner is not null)
        {
            return Result<UserResponse>.Failure(UserErrors.Duplicates(loginOwner?.Login.Value, displayNameOwner?.DisplayName));
        }

        // A profile that does not exist is discarded, not an error (USR-13).
        var existingProfiles = await profiles.ListByIdsAsync(command.ProfileIds, cancellationToken);
        user.AssignProfiles(existingProfiles.Select(profile => profile.Id));

        // The credential joins the same save as the user, so nobody is left registered without a password.
        users.Add(user);
        credentials.AddWithTemporaryPassword(user.Id, command.TemporaryPassword);
        var saved = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return Result<UserResponse>.Failure(saved.Error);
        }

        var displayNames = await users.GetDisplayNamesAsync(user.AuthorIds(), cancellationToken);
        return Result<UserResponse>.Success(user.ToResponse(displayNames));
    }
}
