using FluentValidation;
using ControlService.Application.Commercial.Customers.Commands;

namespace ControlService.Application.Commercial.Customers.Validators;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Identificador do cliente é obrigatório.");

        RuleFor(c => c.PostalCode)
            .NotEmpty().WithMessage("CEP é obrigatório.")
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres.");

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

        RuleFor(c => c.OperationalNote)
            .MaximumLength(500).WithMessage("Nota operacional deve ter no máximo 500 caracteres.")
            .When(c => c.OperationalNote != null);

        RuleFor(c => c.FinancialNote)
            .MaximumLength(500).WithMessage("Nota financeira deve ter no máximo 500 caracteres.")
            .When(c => c.FinancialNote != null);

        RuleFor(c => c.Activity)
            .MaximumLength(150).WithMessage("Atividade deve ter no máximo 150 caracteres.")
            .When(c => c.Activity != null);
    }
}
