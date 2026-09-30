using FluentValidation;

namespace ControlService.Application.Auth.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator(AuthSettings settings)
    {
        RuleFor(command => command.Password)
            .MinimumLength(settings.PasswordMinLength)
            .WithMessage($"A senha precisa ter pelo menos {settings.PasswordMinLength} caracteres.");

        RuleFor(command => command.PasswordConfirmation)
            .Equal(command => command.Password)
            .WithMessage("As duas senhas não são iguais. Digite de novo.");
    }
}
