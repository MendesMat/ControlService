using FluentValidation;
using ControlService.Application.Commercial.Customers.Commands;
using System.Text.RegularExpressions;

namespace ControlService.Application.Commercial.Customers.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.LegalName)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(c => c.PostalCode)
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres.")
            .When(c => c.PostalCode != null);

        RuleFor(c => c.Street)
            .NotEmpty().WithMessage("Logradouro é obrigatório.")
            .MaximumLength(200).WithMessage("Logradouro deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Neighborhood)
            .NotEmpty().WithMessage("Bairro é obrigatório.")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres.");

        RuleFor(c => c.City)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(c => c.State)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve ser a sigla com 2 caracteres (ex: SP).");

        RuleFor(c => c.DocumentValue)
            .Must((value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                var cleanValue = Regex.Replace(value, "[^0-9a-zA-Z]", "");
                return cleanValue.Length == 11 || cleanValue.Length == 14;
            })
            .WithMessage("O documento deve ter 11 (CPF) ou 14 (CNPJ) caracteres.")
            .When(c => !string.IsNullOrWhiteSpace(c.DocumentValue));
    }
}

