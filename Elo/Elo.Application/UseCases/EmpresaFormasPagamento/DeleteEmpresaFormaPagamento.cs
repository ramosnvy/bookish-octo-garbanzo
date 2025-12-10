using Elo.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.EmpresaFormasPagamento;

public static class DeleteEmpresaFormaPagamento
{
    public record Command : IRequest<Unit>
    {
        public int Id { get; set; }
        
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID inválido.");
            RuleFor(x => x.EmpresaId).GreaterThan(0).WithMessage("Empresa inválida.");
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IEmpresaFormaPagamentoService _service;

        public Handler(IEmpresaFormaPagamentoService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _service.DeletarFormaPagamentoAsync(request.Id, request.EmpresaId);
            return Unit.Value;
        }
    }
}
