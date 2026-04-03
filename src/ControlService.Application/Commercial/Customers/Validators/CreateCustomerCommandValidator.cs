using FluentValidation;
using ControlService.Application.Commercial.Customers.Commands;

namespace ControlService.Application.Commercial.Customers.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.LegalName)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

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

        RuleFor(c => c.DocumentType)
            .NotNull().WithMessage("Tipo de documento é obrigatório quando o documento é informado.")
            .When(c => !string.IsNullOrWhiteSpace(c.DocumentValue));

        RuleFor(c => c.DocumentValue)
            .NotEmpty().WithMessage("Documento é obrigatório quando o tipo de documento é informado.")
            .When(c => c.DocumentType.HasValue);
    }
}
