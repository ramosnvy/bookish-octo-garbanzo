using Elo.Application.DTOs.EmpresaFormaPagamento;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.EmpresaFormasPagamento;

public static class CreateEmpresaFormaPagamento
{
    public record Command : IRequest<EmpresaFormaPagamentoDto>
    {
        public FormaPagamento FormaPagamento { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool AVista { get; set; }
        
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FormaPagamento).IsInEnum().WithMessage("Forma de pagamento inválida.");
            RuleFor(x => x.EmpresaId).GreaterThan(0).WithMessage("Empresa inválida.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
        }
    }

    public class Handler : IRequestHandler<Command, EmpresaFormaPagamentoDto>
    {
        private readonly IEmpresaFormaPagamentoService _service;

        public Handler(IEmpresaFormaPagamentoService service)
        {
            _service = service;
        }

        public async Task<EmpresaFormaPagamentoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var formaPagamento = await _service.CriarFormaPagamentoAsync(
                request.EmpresaId,
                request.FormaPagamento,
                request.Nome,
                request.AVista);

            return new EmpresaFormaPagamentoDto
            {
                Id = formaPagamento.Id,
                EmpresaId = formaPagamento.EmpresaId,
                FormaPagamento = formaPagamento.FormaPagamento,
                FormaPagamentoNome = formaPagamento.FormaPagamento.ToString(),
                Nome = formaPagamento.Nome,
                AVista = formaPagamento.AVista,
                Ativo = formaPagamento.Ativo,
                CreatedAt = formaPagamento.CreatedAt
            };
        }
    }
}
