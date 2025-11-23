using System.Collections.Generic;
using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class UpdateEmpresa
{
    public class Command : IRequest<EmpresaDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string EmailContato { get; set; } = string.Empty;
        public string TelefoneContato { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class Handler : IRequestHandler<Command, EmpresaDto>
    {
        private readonly IEmpresaService _empresaService;

        public Handler(IEmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        public async Task<EmpresaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var empresa = await _empresaService.ObterPorIdAsync(request.Id);
            if (empresa == null)
            {
                throw new KeyNotFoundException($"Empresa com ID {request.Id} não encontrada");
            }

            empresa.Nome = request.Nome;
            empresa.Documento = request.Documento;
            empresa.EmailContato = request.EmailContato;
            empresa.TelefoneContato = request.TelefoneContato;
            empresa.Ativo = request.Ativo;
            empresa.UpdatedAt = DateTime.UtcNow;

            var atualizada = await _empresaService.AtualizarEmpresaAsync(empresa);

            return new EmpresaDto
            {
                Id = atualizada.Id,
                Nome = atualizada.Nome,
                Documento = atualizada.Documento,
                EmailContato = atualizada.EmailContato,
                TelefoneContato = atualizada.TelefoneContato,
                Ativo = atualizada.Ativo,
                CreatedAt = atualizada.CreatedAt,
                UpdatedAt = atualizada.UpdatedAt
            };
        }
    }
}
