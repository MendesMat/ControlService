using System.Linq.Expressions;
using ControlService.Application.Auth;
using ControlService.Application.Common;
using ControlService.Domain.Common;
using ControlService.Domain.Users;
using FluentValidation;

namespace ControlService.Application.Users.CreateUser;

/// <summary>The rules of users.md that need only the typed values; the ones that need the database (USR-03, USR-06)
/// are in the handler. A message is verbatim from the feature document: the value objects hold the ones of formats.</summary>
public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(AuthSettings settings, TimeProvider timeProvider)
    {
        RuleFor(command => command.FullName).NotEmpty().WithMessage("Informe o nome completo."); // USR-01
        RuleFor(command => command.DisplayName).NotEmpty().WithMessage("Informe como o nome vai aparecer no sistema."); // USR-02
        RuleFor(command => command.Login).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o login.") // USR-04
            .Custom((login, context) => Check(Login.Create(login), context)); // USR-05
        RuleFor(command => command.TemporaryPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe a senha temporária.") // USR-35
            .MinimumLength(settings.PasswordMinLength)
            .WithMessage($"A senha precisa ter pelo menos {settings.PasswordMinLength} caracteres."); // USR-35, AUTH-16

        RuleFor(command => command.Email).Custom((email, context) => Check(EmailAddress.Create(email), context)); // USR-07
        RuleFor(command => command.BirthDate).Custom((text, context) =>
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!UserInputParsing.TryParseBirthDate(text, out var birthDate))
            {
                context.AddFailure("Esta data de nascimento não é válida."); // USR-37
            }
            else if (birthDate > BrasiliaTime.Today(timeProvider))
            {
                context.AddFailure("A data de nascimento não pode ser no futuro."); // USR-09
            }
        });
        WhenFilledIn(command => command.Cpf, Cpf.Create); // USR-08
        WhenFilledIn(command => command.Phone, PhoneNumber.Create); // USR-10
        WhenFilledIn(command => command.BloodType, BloodType.Create); // USR-26
        WhenFilledIn(command => command.Address.Cep, Cep.Create); // USR-11
        WhenFilledIn(command => command.EmergencyContact.Phone, PhoneNumber.Create); // USR-12
    }

    // An optional field is checked only when something was typed in it.
    private void WhenFilledIn<T>(Expression<Func<CreateUserCommand, string>> field, Func<string, Result<T>> create)
    {
        var read = field.Compile();
        RuleFor(field)
            .Custom((value, context) => Check(create(value), context))
            .When(command => !string.IsNullOrWhiteSpace(read(command)));
    }

    private static void Check<T>(Result<T> result, ValidationContext<CreateUserCommand> context)
    {
        if (result.IsFailure)
        {
            context.AddFailure(result.Error.Message);
        }
    }
}
