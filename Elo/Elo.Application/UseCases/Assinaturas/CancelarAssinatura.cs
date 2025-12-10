using Elo.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.Assinaturas;

public static class CancelarAssinatura
{
    public record Command : IRequest<bool>
    {
        public int AssinaturaId { get; set; }
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AssinaturaId).GreaterThan(0).WithMessage("Assinatura inválida.");
        }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAssinaturaService _assinaturaService;

        public Handler(IAssinaturaService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            await _assinaturaService.CancelarAssinaturaAsync(request.AssinaturaId, request.EmpresaId);
            return true;
        }
    }
}
