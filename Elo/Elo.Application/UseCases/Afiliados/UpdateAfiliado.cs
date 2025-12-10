using MediatR;
using Elo.Application.DTOs.Afiliado;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Afiliados;

public static class UpdateAfiliado
{
    public class Command : IRequest<AfiliadoDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public decimal Porcentagem { get; set; }
        public Elo.Domain.Enums.Status Status { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, AfiliadoDto>
    {
        private readonly IAfiliadoService _afiliadoService;
        private readonly IAfiliadoMapper _mapper;

        public Handler(IAfiliadoService afiliadoService, IAfiliadoMapper mapper)
        {
            _afiliadoService = afiliadoService;
            _mapper = mapper;
        }

        public async Task<AfiliadoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var afiliado = await _afiliadoService.AtualizarAfiliadoAsync(
                request.Id,
                request.Nome,
                request.Email,
                request.Documento,
                request.Telefone,
                request.Porcentagem,
                request.Status,
                request.EmpresaId
            );

            return _mapper.ToDto(afiliado);
        }
    }
}
