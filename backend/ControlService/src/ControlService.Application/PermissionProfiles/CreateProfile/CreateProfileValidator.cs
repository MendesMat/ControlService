using FluentValidation;

namespace ControlService.Application.PermissionProfiles.CreateProfile;

public sealed class CreateProfileValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileValidator()
    {
        RuleFor(command => command.Name).NotEmpty().WithMessage(PermissionProfileErrors.NameRequiredMessage);
    }
}
