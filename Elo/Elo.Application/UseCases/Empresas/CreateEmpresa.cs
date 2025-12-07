using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class CreateEmpresa
{
    public class Command : IRequest<EmpresaDto>
    {
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string EmailContato { get; set; } = string.Empty;
        public string TelefoneContato { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public string UsuarioNome { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public string UsuarioPassword { get; set; } = string.Empty;
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
            var empresa = new Empresa
            {
                Nome = request.Nome,
                Documento = request.Documento,
                EmailContato = request.EmailContato,
                TelefoneContato = request.TelefoneContato,
                Ativo = request.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            var criada = await _empresaService.CriarEmpresaAsync(empresa, request.UsuarioNome, request.UsuarioEmail, request.UsuarioPassword);

            return new EmpresaDto
            {
                Id = criada.Id,
                Nome = criada.Nome,
                Documento = criada.Documento,
                EmailContato = criada.EmailContato,
                TelefoneContato = criada.TelefoneContato,
                Ativo = criada.Ativo,
                CreatedAt = criada.CreatedAt,
                UpdatedAt = criada.UpdatedAt
            };
        }
    }
}
