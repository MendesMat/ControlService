using FluentValidation;

namespace ControlService.Application.PermissionProfiles.UpdateProfile;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(command => command.Name).NotEmpty().WithMessage(PermissionProfileErrors.NameRequiredMessage);
    }
}
