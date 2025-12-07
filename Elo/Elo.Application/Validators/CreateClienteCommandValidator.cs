using FluentValidation;
using Elo.Application.UseCases.Clientes;

namespace Elo.Application.Validators;

public class CreateClienteCommandValidator : AbstractValidator<CreateCliente.Command>
{
    public CreateClienteCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.CnpjCpf)
            .NotEmpty().WithMessage("CNPJ/CPF é obrigatório")
            .MaximumLength(20).WithMessage("CNPJ/CPF deve ter no máximo 20 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(BeValidStatus).WithMessage("Status deve ser um valor válido");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<Domain.Enums.Status>(status, out _);
    }
}
