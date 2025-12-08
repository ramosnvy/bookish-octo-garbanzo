using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class CreateEmpresa
{
    public class Command : IRequest<EmpresaDto>
    {
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Ie { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
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
                RazaoSocial = request.RazaoSocial,
                NomeFantasia = request.NomeFantasia,
                Cnpj = request.Cnpj,
                Ie = request.Ie,
                Email = request.Email,
                Telefone = request.Telefone,
                Endereco = request.Endereco,
                Ativo = request.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            var criada = await _empresaService.CriarEmpresaAsync(empresa);

            return new EmpresaDto
            {
                Id = criada.Id,
                RazaoSocial = criada.RazaoSocial,
                NomeFantasia = criada.NomeFantasia,
                Cnpj = criada.Cnpj,
                Ie = criada.Ie,
                Email = criada.Email,
                Telefone = criada.Telefone,
                Endereco = criada.Endereco,
                Ativo = criada.Ativo,
                CreatedAt = criada.CreatedAt,
                UpdatedAt = criada.UpdatedAt
            };
        }
    }
}
