using FluentValidation;
using Elo.Application.DTOs.Afiliado;

namespace Elo.Application.Validators;

public class CreateAfiliadoDtoValidator : AbstractValidator<CreateAfiliadoDto>
{
    public CreateAfiliadoDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("Documento é obrigatório")
            .MaximumLength(20).WithMessage("Documento deve ter no máximo 20 caracteres")
            .Matches(@"^[0-9]+$").WithMessage("Documento deve conter apenas números");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres");

        RuleFor(x => x.Porcentagem)
            .GreaterThanOrEqualTo(0m).WithMessage("Porcentagem deve ser maior ou igual a 0")
            .LessThanOrEqualTo(100m).WithMessage("Porcentagem deve ser menor ou igual a 100");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido");
    }
}
