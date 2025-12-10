using Elo.Application.DTOs.EmpresaFormaPagamento;
using Elo.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.EmpresaFormasPagamento;

public static class GetAllEmpresaFormasPagamento
{
    public record Query : IRequest<List<EmpresaFormaPagamentoDto>>
    {
        public bool ApenasAtivos { get; set; } = true;
        
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.EmpresaId).GreaterThan(0).WithMessage("Empresa inválida.");
        }
    }

    public class Handler : IRequestHandler<Query, List<EmpresaFormaPagamentoDto>>
    {
        private readonly IEmpresaFormaPagamentoService _service;

        public Handler(IEmpresaFormaPagamentoService service)
        {
            _service = service;
        }

        public async Task<List<EmpresaFormaPagamentoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var formasPagamento = await _service.ObterFormasPagamentoPorEmpresaAsync(
                request.EmpresaId,
                request.ApenasAtivos);

            return formasPagamento.Select(fp => new EmpresaFormaPagamentoDto
            {
                Id = fp.Id,
                EmpresaId = fp.EmpresaId,
                FormaPagamento = fp.FormaPagamento,
                FormaPagamentoNome = fp.FormaPagamento.ToString(),
                Nome = fp.Nome,
                AVista = fp.AVista,
                Ativo = fp.Ativo,
                CreatedAt = fp.CreatedAt
            }).ToList();
        }
    }
}
